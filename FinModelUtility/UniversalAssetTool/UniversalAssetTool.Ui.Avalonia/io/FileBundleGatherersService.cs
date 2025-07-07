using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

using Avalonia;

using fin.io.bundles;
using fin.util.asserts;
using fin.util.progress;

using uni.games;
using uni.ui.avalonia.common.progress;
using uni.ui.avalonia.common.treeViews;
using uni.ui.avalonia.util;
using uni.ui.winforms.common.fileTreeView;

namespace uni.ui.avalonia.io;

public static class FileBundleGatherersService {
  private static
      IReadOnlyList<(INamedAnnotatedFileBundleGatherer gatherer,
          IPercentageProgress progress)> gatherersAndProgresses_;

  public static ValueFractionProgress StartExtracting() {
    var valueFractionProgress = new ValueFractionProgress();

    var splitProgress = valueFractionProgress.AsValueless().Split(2);
    var loadingProgress = splitProgress[0];
    var fileTreeProgress = splitProgress[1];

    Task.Run(() => {
      var rootDirectory = new RootFileBundleGatherer()
          .GatherAllFiles(
              loadingProgress,
              out var gatherersAndProgresses);
      gatherersAndProgresses_ = gatherersAndProgresses;

      var totalNodeCount
          = GetTotalNodeCountWithinDirectory_(rootDirectory);
      var counterProgress = new CounterPercentageProgress(totalNodeCount);
      counterProgress.OnProgressChanged += (_, progress)
          => fileTreeProgress.ReportProgress(progress);

      var fileTreeViewModel
          = GetFileTreeViewModel_(rootDirectory,
                                  counterProgress);
      valueFractionProgress.ReportCompletion(fileTreeViewModel);
    });

    return valueFractionProgress;
  }

  private static int GetTotalNodeCountWithinDirectory_(
      IFileBundleDirectory directoryRoot)
    => 1 +
       directoryRoot.Subdirs.Sum(GetTotalNodeCountWithinDirectory_) +
       directoryRoot.FileBundles.Count;

  private static FileBundleTreeViewModel GetFileTreeViewModel_(
      IFileBundleDirectory directoryRoot,
      CounterPercentageProgress counterPercentageProgress) {
    var viewModel = new FileBundleTreeViewModel(
        new ObservableCollection<INode<IAnnotatedFileBundle>>(
            directoryRoot
                .Subdirs
                .Select(subdir => CreateDirectoryNode_(
                            subdir,
                            counterPercentageProgress))));

    viewModel.NodeSelected
        += (_, node) => {
          switch (node) {
            case FileBundleLeafNode leafNode: {
              SelectedFileTreeDirectoryService.SelectFileTreeDirectory(
                  leafNode.Parent.AssertNonnull());
              FileBundleService.OpenFileBundle(null, leafNode.Value);
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

  private static INode<IAnnotatedFileBundle> CreateDirectoryNode_(
      IFileBundleDirectory directory,
      CounterPercentageProgress counterPercentageProgress,
      IList<string>? parts = null) {
    counterPercentageProgress.Increment();

    var subdirs = directory.Subdirs;
    var fileBundles = directory.FileBundles;

    var subdirCount = subdirs.Count;
    var fileBundlesCount = fileBundles.Count;

    if (subdirCount + fileBundlesCount == 1) {
      parts ??= new List<string>();
      parts.Add(directory.Name);

      return subdirCount == 1
          ? CreateDirectoryNode_(subdirs[0],
                                 counterPercentageProgress,
                                 parts)
          : CreateFileNode_(fileBundles[0],
                            counterPercentageProgress,
                            parts);
    }

    string text = directory.Name;
    if (parts != null) {
      parts.Add(text);
      text = string.Join('/', parts.ToArray());
    }

    return new FileBundleDirectoryNode(
        text,
        new ObservableCollection<INode<IAnnotatedFileBundle>>(
            directory
                .Subdirs
                .Select(d => CreateDirectoryNode_(
                            d,
                            counterPercentageProgress))
                .Concat(
                    directory.FileBundles.Select(f => CreateFileNode_(
                                                     f,
                                                     counterPercentageProgress)))));
  }

  private static INode<IAnnotatedFileBundle> CreateFileNode_(
      IAnnotatedFileBundle fileBundle,
      CounterPercentageProgress counterPercentageProgress,
      IList<string>? parts = null) {
    counterPercentageProgress.Increment();

    var displayName = fileBundle.FileBundle.DisplayName.ToString();

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