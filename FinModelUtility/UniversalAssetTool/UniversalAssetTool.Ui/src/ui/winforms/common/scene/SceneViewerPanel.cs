using System;
using System.Windows.Forms;

using fin.animation;
using fin.importers;
using fin.io.bundles;
using fin.model;
using fin.scene;
using fin.ui.rendering;
using fin.ui.rendering.gl.model;

namespace uni.ui.winforms.common.scene {
  public partial class SceneViewerPanel : UserControl, ISceneViewer {
    public SceneViewerPanel() {
      this.InitializeComponent();
    }

    public (I3dFileBundle, IScene)? FileBundleAndScene {
      get => this.impl_.FileBundleAndScene;
      set {
        var fileBundle = value?.Item1;
        if (fileBundle != null) {
          this.groupBox_.Text = fileBundle.DisplayFullPath;
        } else {
          this.groupBox_.Text = "(Select a model)";
        }

        this.impl_.FileBundleAndScene = value;
      }
    }

    public ISceneModel? FirstSceneModel => this.impl_.FirstSceneModel;

    public IAnimationPlaybackManager? AnimationPlaybackManager 
      => this.impl_.AnimationPlaybackManager;

    public ISkeletonRenderer? SkeletonRenderer => this.impl_.SkeletonRenderer;

    public IReadOnlyModelAnimation? Animation {
      get => this.impl_.Animation;
      set => this.impl_.Animation = value;
    }

    public TimeSpan FrameTime => this.impl_.FrameTime;
  }
}