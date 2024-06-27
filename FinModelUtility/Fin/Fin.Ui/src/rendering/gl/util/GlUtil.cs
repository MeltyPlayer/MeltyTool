using OpenTK.Graphics.OpenGL;

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
      GL.ShadeModel(ShadingModel.Smooth);

      GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Fill);

      GL.ClearDepth(5.0F);

      GL.Hint(HintTarget.PerspectiveCorrectionHint, HintMode.Nicest);

      GL.Enable(EnableCap.Normalize);

      GL.Enable(EnableCap.PrimitiveRestart);
      GL.Enable(EnableCap.PrimitiveRestartFixedIndex);

      GlUtil.ResetBlending();
      GlUtil.ResetClearColor();
      GlUtil.ResetCulling();
      GlUtil.ResetDepth();
      GlUtil.ResetFlipFaces();
    }
}