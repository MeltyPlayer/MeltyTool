using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using fin.io.bundles;
using fin.model;

using ReactiveUI;

using uni.games;
using uni.ui.avalonia.common.progress;
using uni.ui.avalonia.common.treeViews;
using uni.ui.avalonia.resources.model;

namespace uni.ui.avalonia.ViewModels;

public class MainViewModelForDesigner {
  public AsyncPanelViewModel FileBundleTreeAsyncPanelViewModel { get; }

  public ModelPanelViewModel ModelPanel { get; }
    = new ModelPanelViewModelForDesigner();

  public string FileName => "//foo/bar.mod";

  public MainViewModelForDesigner() {
    var asyncProgress = new AsyncProgress();

    Task.Run(() => {
      var fileTreeViewModel = new FileBundleTreeViewModelForDesigner();
      Thread.Sleep(TimeSpan.FromSeconds(3));
      asyncProgress.ReportCompletion(fileTreeViewModel);
    });

    this.FileBundleTreeAsyncPanelViewModel = new AsyncPanelViewModel
        { Progress = asyncProgress };
    ;
  }
}

public class MainViewModel : ViewModelBase {
  private AsyncPanelViewModel fileTreeAsyncPanelViewModel_;
  private string fileName_;
  private ModelPanelViewModel modelPanel_;

  public MainViewModel() {
    var asyncProgress = new AsyncProgress();
    Task.Run(() => {
      var rootDirectory = new RootFileBundleGatherer().GatherAllFiles();
      var fileTreeViewModel = this.GetFileTreeViewModel_(rootDirectory);
      asyncProgress.ReportCompletion(fileTreeViewModel);
    });

    this.FileBundleTreeAsyncPanelViewModel = new AsyncPanelViewModel {
        Progress = asyncProgress
    };


    this.ModelPanel = new ModelPanelViewModel();
    SceneInstanceService.OnSceneInstanceOpened
        += (_, sceneInstance) => {
          this.FileName
              = sceneInstance.Definition.FileBundle?.DisplayFullPath;

          var sceneModelInstances
              = sceneInstance
                .Areas
                .SelectMany(a => a.Objects)
                .SelectMany(o => o.Models)
                .ToArray();

          if (sceneModelInstances.Length == 1) {
            var sceneModelInstance = sceneModelInstances.Single();
            var model = sceneModelInstance.Model;
            var animationPlaybackManager
                = sceneModelInstance.AnimationPlaybackManager;

            this.ModelPanel = new ModelPanelViewModel { Model = model };

            var animationsPanel = this.ModelPanel.AnimationsPanel;
            animationsPanel.AnimationPlaybackManager
                = animationPlaybackManager;
            animationsPanel.OnAnimationSelected
                += (_, animation)
                    => sceneModelInstance.Animation
                        = animation as IReadOnlyModelAnimation;
          } else {
            this.ModelPanel = null;
          }
        };
  }

  public AsyncPanelViewModel FileBundleTreeAsyncPanelViewModel {
    get => this.fileTreeAsyncPanelViewModel_;
    private set
      => this.RaiseAndSetIfChanged(ref this.fileTreeAsyncPanelViewModel_,
                                   value);
  }

  public string FileName {
    get => this.fileName_;

    set => this.RaiseAndSetIfChanged(ref this.fileName_, value);
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