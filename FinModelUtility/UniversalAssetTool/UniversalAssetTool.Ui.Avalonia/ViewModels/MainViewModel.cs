using System;
using System.Linq;
using System.Threading.Tasks;

using fin.model;
using fin.ui.avalonia;

using ReactiveUI;

using uni.ui.avalonia.common.progress;
using uni.ui.avalonia.common.treeViews;
using uni.ui.avalonia.icons;
using uni.ui.avalonia.io;
using uni.ui.avalonia.resources.audio;
using uni.ui.avalonia.resources.model;
using uni.ui.avalonia.toolbars;

namespace uni.ui.avalonia.ViewModels;

public class MainViewModelForDesigner {
  public ProgressPanelViewModel FileBundleTreeAsyncPanelViewModel { get; }

  public AudioPlayerPanelViewModel AudioPlayerPanel { get; } = new();

  public ModelPanelViewModel ModelPanel { get; }
    = new ModelPanelViewModelForDesigner();

  public FileBundleToolbarModel FileBundleToolbar { get; }
    = new FileBundleToolbarModelForDesigner();

  public TopToolbarModel TopToolbar { get; }
    = new TopToolbarModelForDesigner();

  public MainViewModelForDesigner() {
    var progress = new ValueFractionProgress();

    var secondsToWait = 3;
    var start = DateTime.Now;

    Task.Run(async () => {
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
  public AudioPlayerPanelViewModel AudioPlayerPanel { get; } = new();

  public MainViewModel() {
    this.FileBundleTreeAsyncPanelViewModel = new ProgressPanelViewModel {
        Progress = FileBundleGatherersService.StartExtracting()
    };

    this.ModelPanel = new ModelPanelViewModel();
    SceneInstanceService.OnSceneInstanceOpened
        += (_, sceneInstance) => {
          AvaloniaIconUtil.ClearCache();

          var fileBundle = sceneInstance.Definition.FileBundle;
          this.FileBundleToolbar = new FileBundleToolbarModel {
              FileName = fileBundle?.DisplayFullPath.ToString(),
          };
          this.TopToolbar = new TopToolbarModel {
              FileBundle = fileBundle,
          };

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
        += playlist => { this.AudioPlayerPanel.AudioFileBundles = playlist; };
  }

  public ProgressPanelViewModel FileBundleTreeAsyncPanelViewModel {
    get;
    private set
      => this.RaiseAndSetIfChanged(ref field,
                                   value);
  }

  public FileBundleToolbarModel FileBundleToolbar {
    get;
    set => this.RaiseAndSetIfChanged(ref field, value);
  }

  public TopToolbarModel TopToolbar {
    get;
    set => this.RaiseAndSetIfChanged(ref field, value);
  }

  public ModelPanelViewModel ModelPanel {
    get;
    private set => this.RaiseAndSetIfChanged(
        ref field,
        value);
  }
}