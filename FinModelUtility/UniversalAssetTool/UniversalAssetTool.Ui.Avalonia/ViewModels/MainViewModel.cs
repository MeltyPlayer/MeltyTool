using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using fin.io.bundles;
using fin.model;
using fin.util.progress;

using ReactiveUI;

using uni.games;
using uni.ui.avalonia.common.progress;
using uni.ui.avalonia.common.treeViews;
using uni.ui.avalonia.resources.audio;
using uni.ui.avalonia.resources.model;

namespace uni.ui.avalonia.ViewModels;

public class MainViewModelForDesigner {
  public ProgressPanelViewModel FileBundleTreeAsyncPanelViewModel { get; }

  public AudioPlayerPanelViewModel AudioPlayerPanel { get; } = new();

  public ModelPanelViewModel ModelPanel { get; }
    = new ModelPanelViewModelForDesigner();

  public string FileName => "//foo/bar.mod";

  public MainViewModelForDesigner() {
    var progress = new ValueFractionProgress();

    var secondsToWait = 3;
    var start = DateTime.Now;

    Task.Run(
        async () => {
          DateTime current;
          double elapsedSeconds;
          do {
            current = DateTime.Now;
            elapsedSeconds = (current - start).TotalSeconds;
            progress.ReportProgress(
                100 *
                Math.Clamp((float) (elapsedSeconds / secondsToWait), 0, 1));

            await Task.Delay(50);
          } while (elapsedSeconds < secondsToWait);

          var fileTreeViewModel = new FileBundleTreeViewModelForDesigner();
          progress.ReportCompletion(fileTreeViewModel);
        });

    this.FileBundleTreeAsyncPanelViewModel = new ProgressPanelViewModel
        { Progress = progress };
    ;
  }
}

public class MainViewModel : ViewModelBase {
  private ProgressPanelViewModel fileTreeAsyncPanelViewModel_;
  private string fileName_;
  private ModelPanelViewModel modelPanel_;

  public AudioPlayerPanelViewModel AudioPlayerPanel { get; } = new();

  public MainViewModel() {
    var valueFractionProgress = new ValueFractionProgress();

    var splitProgress = valueFractionProgress.AsValueless().Split(2);
    var loadingProgress = splitProgress[0];
    var fileTreeProgress = splitProgress[1];

    Task.Run(() => {
      var rootDirectory
          = new RootFileBundleGatherer().GatherAllFiles(loadingProgress);

      var totalNodeCount = this.GetTotalNodeCountWithinDirectory_(rootDirectory);
      var counterProgress = new CounterPercentageProgress(totalNodeCount);
      counterProgress.OnProgressChanged += (_, progress)
          => fileTreeProgress.ReportProgress(progress);

      var fileTreeViewModel
          = this.GetFileTreeViewModel_(rootDirectory, counterProgress);
      valueFractionProgress.ReportCompletion(fileTreeViewModel);
    });

    this.FileBundleTreeAsyncPanelViewModel = new ProgressPanelViewModel {
        Progress = valueFractionProgress
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

    AudioPlaylistService.OnPlaylistUpdated
        += playlist => {
          this.AudioPlayerPanel.AudioFileBundles = playlist;
        };
  }

  public ProgressPanelViewModel FileBundleTreeAsyncPanelViewModel {
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

  private int GetTotalNodeCountWithinDirectory_(
      IFileBundleDirectory directoryRoot)
    => 1 +
       directoryRoot.Subdirs.Sum(this.GetTotalNodeCountWithinDirectory_) +
       directoryRoot.FileBundles.Count;

  private FileBundleTreeViewModel<IAnnotatedFileBundle> GetFileTreeViewModel_(
      IFileBundleDirectory directoryRoot,
      CounterPercentageProgress counterPercentageProgress) {
    var viewModel = new FileBundleTreeViewModel<IAnnotatedFileBundle> {
        Nodes = new ObservableCollection<INode<IAnnotatedFileBundle>>(
            directoryRoot
                .Subdirs
                .Select(subdir => this.CreateDirectoryNode_(
                            subdir,
                            counterPercentageProgress)))
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
          ? this.CreateDirectoryNode_(subdirs[0],
                                      counterPercentageProgress,
                                      parts)
          : this.CreateFileNode_(fileBundles[0],
                                 counterPercentageProgress,
                                 parts);
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
                .Subdirs.Select(d => this.CreateDirectoryNode_(d, counterPercentageProgress))
                .Concat(
                    directory.FileBundles.Select(
                        f => this.CreateFileNode_(f, counterPercentageProgress)))));
  }

  private INode<IAnnotatedFileBundle> CreateFileNode_(
      IAnnotatedFileBundle fileBundle,
      CounterPercentageProgress counterPercentageProgress,
      IList<string>? parts = null) {
    counterPercentageProgress.Increment();

    string? text = null;
    if (parts != null) {
      parts.Add(fileBundle.FileBundle.DisplayName);
      text = Path.Join(parts.ToArray());
    }

    return new FileBundleLeafNode(text ?? fileBundle.FileBundle.DisplayName,
                                  fileBundle);
  }
}