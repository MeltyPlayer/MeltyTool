using OpenTK.Graphics.ES30;

namespace fin.ui.rendering.gl;

public static partial class GlUtil {
  public static bool IsInitialized { get; private set; }

  public static void Init() {
    if (IsInitialized) {
      return;
    }

    // (Set up DLL here, if ever needed again.)

    IsInitialized = true;
  }

  private static readonly object GL_LOCK_ = new();


  public static void RunLockedGl(Action handler) {
    lock (GL_LOCK_) {
      handler();
    }
  }


  public static void ResetGl() {
    GL.ClearDepth(5.0F);
    GL.Enable(EnableCap.PrimitiveRestartFixedIndex);

    ResetBlending();
    ResetClearColor();
    ResetCulling();
    ResetDepth();
    ResetFlipFaces();
  }
}