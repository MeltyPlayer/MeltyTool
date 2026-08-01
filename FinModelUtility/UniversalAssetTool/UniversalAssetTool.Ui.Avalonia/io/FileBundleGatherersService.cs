using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

using Avalonia;
using Avalonia.Threading;

using fin.io.bundles;
using fin.util.progress;
using fin.util.tasks;

using uni.games;
using uni.services;
using uni.ui.avalonia.common.progress;
using uni.ui.avalonia.common.treeViews;
using uni.ui.avalonia.util;
using uni.ui.winforms.common.fileTreeView;

namespace uni.ui.avalonia.io;

public static class FileBundleGatherersService {
  private static
      IReadOnlyList<(INamedFileBundleGatherer gatherer,
          IPercentageProgress progress)> gatherersAndProgresses_;

  public static ValueFractionProgress StartExtracting() {
    var valueFractionProgress = new ValueFractionProgress();
    var loadingProgress = valueFractionProgress.AsValueless();

    Dispatcher.CurrentDispatcher.InvokeAsync(async () => {
      var (rootDirectory, pbYielder) = await FinTask.Run(() => {
        var rootDirectory = new RootFileBundleGatherer()
            .GatherAllFiles(
                loadingProgress,
                out var gatherersAndProgresses);
        gatherersAndProgresses_ = gatherersAndProgresses;

        var totalNodeCount
            = GetTotalNodeCountWithinDirectory_(rootDirectory);
        var pbYielder = new PercentageBasedYielder(totalNodeCount);

        return (rootDirectory, pbYielder);
      });

      var fileTreeViewModel = await GetFileTreeViewModel_(rootDirectory, pbYielder);
      valueFractionProgress.ReportCompletion(fileTreeViewModel);
    }, DispatcherPriority.MaxValue);

    return valueFractionProgress;
  }

  private static int GetTotalNodeCountWithinDirectory_(
      IFileBundleDirectory directoryRoot)
    => 1 +
       directoryRoot.Subdirs.Sum(GetTotalNodeCountWithinDirectory_) +
       directoryRoot.FileBundles.Count;

  private static async Task<FileBundleTreeViewModel> GetFileTreeViewModel_(
      IFileBundleDirectory directoryRoot,
      PercentageBasedYielder pbYielder) {
    var rootSubdirs = await Task.WhenAll(directoryRoot
          .Subdirs
          .Select(subdir => CreateDirectoryNode_(
                      subdir,
                      pbYielder)));

    var viewModel = new FileBundleTreeViewModel(
        new ObservableCollection<INode<IFileBundle>>(rootSubdirs));

    viewModel.NodeSelected
        += (_, node) => {
          switch (node) {
            case FileBundleLeafNode leafNode: {
              if (leafNode.Parent != null) {
                SelectedFileTreeDirectoryService.SelectFileTreeDirectory(
                    leafNode.Parent);
              }
              FileBundleService.OpenFileBundle(leafNode, leafNode.Value);
              break;
            }
            case IFileTreeParentNode parentNode: {
              SelectedFileTreeDirectoryService.SelectFileTreeDirectory(
                  parentNode);
              break;
            }
          }
        };

    return viewModel;
  }

  private static async Task<INode<IFileBundle>> CreateDirectoryNode_(
      IFileBundleDirectory directory,
      PercentageBasedYielder pbYielder,
      IList<string>? parts = null) {
    await pbYielder.IncrementAsync();

    var subdirs = directory.Subdirs;
    var fileBundles = directory.FileBundles;

    var subdirCount = subdirs.Count;
    var fileBundlesCount = fileBundles.Count;

    if (subdirCount + fileBundlesCount == 1) {
      parts ??= new List<string>();
      parts.Add(directory.Name);

      return subdirCount == 1
          ? await CreateDirectoryNode_(subdirs[0],
                                 pbYielder,
                                 parts)
          : await CreateFileNode_(fileBundles[0],
                            pbYielder,
                            parts);
    }

    string text = directory.Name;
    if (parts != null) {
      parts.Add(text);
      text = string.Join('/', parts.ToArray());
    }


    var subnodes = new INode<IFileBundle>[subdirCount + fileBundlesCount];
    var i = 0;
    for (var d = 0; d < subdirCount; ++d) {
      subnodes[i++] = await CreateDirectoryNode_(
          subdirs[d],
          pbYielder);
    }
    for (var f = 0; f < fileBundlesCount; ++f) {
      subnodes[i++] = await CreateFileNode_(
          fileBundles[f],
          pbYielder);
    }

    return new FileBundleDirectoryNode(text, subnodes);
  }

  private static async Task<INode<IFileBundle>> CreateFileNode_(
      IFileBundle fileBundle,
      PercentageBasedYielder pbYielder,
      IList<string>? parts = null) {
    await pbYielder.IncrementAsync();

    var displayName = fileBundle.DisplayName.ToString();

    string? text = null;
    if (parts != null) {
      parts.Add(displayName);
      text = string.Join('/', parts.ToArray());
    }

    var label = text ?? displayName;
    return new FileBundleLeafNode(label, fileBundle);
  }

  public static void ShowExtractorProgressWindow(Visual visual) {
    visual.ShowNewWindow(() => new FileBundleGatherersProgressWindow {
        DataContext = new FileBundleGatherersProgressViewModel {
            FileBundleGatherers
                = gatherersAndProgresses_
                  .Select(t => new FileBundleGathererProgressViewModel(
                              t.gatherer.Name,
                              t.progress))
                  .ToArray()
        }
    });
  }
}