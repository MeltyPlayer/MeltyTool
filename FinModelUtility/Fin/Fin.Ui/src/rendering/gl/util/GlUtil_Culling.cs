using fin.model;

using OpenTK.Graphics.OpenGL4;

namespace fin.ui.rendering.gl;

public partial record GlState {
  public CullingMode CurrentCullingMode { get; set; } =
    CullingMode.SHOW_NEITHER;
}

public static partial class GlUtil {
  public static void ResetCulling()
    => SetCulling(CullingMode.SHOW_FRONT_ONLY);


  public static bool SetCulling(CullingMode cullingMode) {
    if (currentState_.CurrentCullingMode == cullingMode) {
      return false;
    }

    currentState_.CurrentCullingMode = cullingMode;

    if (cullingMode == CullingMode.SHOW_BOTH) {
      GL.Disable(EnableCap.CullFace);
    } else {
      GL.Enable(EnableCap.CullFace);
      GL.CullFace(cullingMode switch {
          CullingMode.SHOW_FRONT_ONLY => TriangleFace.Back,
          CullingMode.SHOW_BACK_ONLY  => TriangleFace.Front,
          CullingMode.SHOW_NEITHER    => TriangleFace.FrontAndBack,
          _ => throw new ArgumentOutOfRangeException(
              nameof(cullingMode),
              cullingMode,
              null)
      });
    }

    return true;
  }
}