using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;

using fin.io.bundles;

using ReactiveUI;

using uni.games;
using uni.ui.avalonia.common.treeViews;
using uni.ui.avalonia.resources.model;

namespace uni.ui.avalonia.ViewModels;

public class MainViewModelForDesigner {
  public FileBundleTreeViewModelForDesigner FileBundleTreeViewModelForDesigner { get; }
    = new();

  public ModelPanelViewModel ModelPanel { get; }
    = new ModelPanelViewModelForDesigner();
}

public class MainViewModel : ViewModelBase {
  private FileBundleTreeViewModel<IAnnotatedFileBundle> fileTree_;
  private ModelPanelViewModel modelPanel_;

  public MainViewModel() {
    var rootDirectory = new RootFileBundleGatherer().GatherAllFiles();
    this.FileBundleTreeViewModel = this.GetFileTreeViewModel_(rootDirectory);

    this.ModelPanel = new ModelPanelViewModel();
    ModelService.OnModelOpened
        += (_, model) => {
             this.ModelPanel = new ModelPanelViewModel { Model = model };
           };
  }

  public FileBundleTreeViewModel<IAnnotatedFileBundle> FileBundleTreeViewModel {
    get => this.fileTree_;
    private set => this.RaiseAndSetIfChanged(ref this.fileTree_, value);
  }

  public ModelPanelViewModel ModelPanel {
    get => this.modelPanel_;
    private set => this.RaiseAndSetIfChanged(
        ref this.modelPanel_,
        value);
  }


  private FileBundleTreeViewModel<IAnnotatedFileBundle> GetFileTreeViewModel_(
      IFileBundleDirectory directoryRoot) {
    var viewModel = new FileBundleTreeViewModel<IAnnotatedFileBundle> {
        Nodes = new ObservableCollection<INode<IAnnotatedFileBundle>>(
            directoryRoot
                .Subdirs
                .Select(subdir => this.CreateDirectoryNode_(subdir)))
    };

    viewModel.NodeSelected
        += (_, node) => {
             if (node is FileBundleLeafNode leafNode) {
               FileBundleService.OpenFileBundle(null, leafNode.Data.FileBundle);
             }
           };

    return viewModel;
  }

  private INode<IAnnotatedFileBundle> CreateDirectoryNode_(
      IFileBundleDirectory directory,
      IList<string>? parts = null) {
    var subdirs = directory.Subdirs;
    var fileBundles = directory.FileBundles;

    var subdirCount = subdirs.Count;
    var fileBundlesCount = fileBundles.Count;

    if (subdirCount + fileBundlesCount == 1) {
      parts ??= new List<string>();
      parts.Add(directory.Name);

      return subdirCount == 1
          ? this.CreateDirectoryNode_(subdirs[0], parts)
          : this.CreateFileNode_(fileBundles[0], parts);
    }

    string text = directory.Name;
    if (parts != null) {
      parts.Add(text);
      text = Path.Join(parts.ToArray());
    }

    return new FileBundleDirectoryNode(
        text,
        new ObservableCollection<INode<IAnnotatedFileBundle>>(
            directory
                .Subdirs.Select(d => this.CreateDirectoryNode_(d))
                .Concat(
                    directory.FileBundles.Select(
                        f => this.CreateFileNode_(f)))));
  }

  private INode<IAnnotatedFileBundle> CreateFileNode_(
      IAnnotatedFileBundle fileBundle,
      IList<string>? parts = null) {
    string? text = null;
    if (parts != null) {
      parts.Add(fileBundle.FileBundle.DisplayName);
      text = Path.Join(parts.ToArray());
    }

    return new FileBundleLeafNode(text ?? fileBundle.FileBundle.DisplayName,
                                  fileBundle);
  }
}