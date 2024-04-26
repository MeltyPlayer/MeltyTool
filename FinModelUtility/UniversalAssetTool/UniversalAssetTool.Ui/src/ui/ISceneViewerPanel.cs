using System;

using fin.animation;
using fin.importers;
using fin.model;
using fin.scene;
using fin.ui.rendering.gl.model;

namespace uni.ui {
  public interface ISceneViewerPanel {
    (I3dFileBundle, IScene)? FileBundleAndScene { get; set; }

    ISceneModel? FirstSceneModel { get; }
    IAnimationPlaybackManager? AnimationPlaybackManager { get; }
    IReadOnlyModelAnimation? Animation { get; set; }
    ISkeletonRenderer? SkeletonRenderer { get; }

    TimeSpan FrameTime { get; }
  }
}