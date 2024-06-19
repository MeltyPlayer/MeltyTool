using fin.animation;
using fin.model;
using fin.scene;
using fin.ui.rendering.gl.model;

namespace fin.ui.rendering {
  public interface ISceneViewer {
    ISceneInstance? Scene { get; set; }

    ISceneModelInstance? FirstSceneModel { get; }
    IAnimationPlaybackManager? AnimationPlaybackManager { get; }
    IReadOnlyModelAnimation? Animation { get; set; }
    ISkeletonRenderer? SkeletonRenderer { get; }
  }
}