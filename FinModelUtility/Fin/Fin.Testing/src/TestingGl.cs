using fin.ui.rendering.gl;

using OpenTK.Graphics.OpenGL;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace fin.testing;

public static class TestingGl {
  public static void InitOneTimeSetup() => HeadlessGl.MakeCurrent();

  public static void InitBeforeEach() {
    GlUtil.ResetGl();

    GL.Disable(EnableCap.Dither);
    GL.Disable(EnableCap.LineSmooth);
    GL.Disable(EnableCap.PolygonSmooth);
    GL.Hint(HintTarget.LineSmoothHint, HintMode.DontCare);
    GL.Hint(HintTarget.PolygonSmoothHint, HintMode.DontCare);
    GL.Disable(EnableCap.Multisample);

    GLFW.WindowHint(WindowHintInt.Samples, 1);
  }
}