using fin.animation;
using fin.importers;
using fin.model;
using fin.scene;
using fin.ui.rendering.gl.model;

namespace fin.ui.rendering {
  public interface ISceneViewer {
    (I3dFileBundle, ISceneInstance)? FileBundleAndScene { get; set; }

    ISceneModelInstance? FirstSceneModel { get; }
    IAnimationPlaybackManager? AnimationPlaybackManager { get; }
    IReadOnlyModelAnimation? Animation { get; set; }
    ISkeletonRenderer? SkeletonRenderer { get; }

    TimeSpan FrameTime { get; }
  }
}