using System.Windows.Forms;

using fin.animation;
using fin.model;
using fin.scene;
using fin.ui.rendering;
using fin.ui.rendering.gl.model;

namespace uni.ui.winforms.common.scene;

public partial class SceneViewerPanel : UserControl, ISceneViewer {
  public SceneViewerPanel() {
      this.InitializeComponent();
    }

  public ISceneInstance? Scene {
    get => this.impl_.Scene;
    set {
        var fileBundle = value?.Definition.FileBundle;
        if (fileBundle != null) {
          this.groupBox_.Text = fileBundle.DisplayFullPath.ToString();
        } else {
          this.groupBox_.Text = "(Select a model)";
        }

        this.impl_.Scene = value;
      }
  }

  public ISceneModelInstance? FirstSceneModel => this.impl_.FirstSceneModel;

  public IAnimationPlaybackManager? AnimationPlaybackManager 
    => this.impl_.AnimationPlaybackManager;

  public ISkeletonRenderer? SkeletonRenderer => this.impl_.SkeletonRenderer;

  public IReadOnlyModelAnimation? Animation {
    get => this.impl_.Animation;
    set => this.impl_.Animation = value;
  }
}