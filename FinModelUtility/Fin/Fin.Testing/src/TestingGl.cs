using fin.ui.rendering.gl;

using OpenTK.Graphics.OpenGL;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace fin.testing;

public static class TestingGl {
  public static void InitOneTimeSetup() => HeadlessGl.MakeCurrent();
  public static void InitBeforeEach() => GlUtil.ResetGl();
}