using fin.ui.rendering.gl;

namespace fin.testing;

public static class TestingGl {
  public static void InitOneTimeSetup() => HeadlessGl.MakeCurrent();
  public static void InitBeforeEach() => GlUtil.ResetGl();
}