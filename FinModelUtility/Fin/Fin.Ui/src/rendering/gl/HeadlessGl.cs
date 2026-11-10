using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;


namespace fin.ui.rendering.gl;

public static class HeadlessGl {
  private static GameWindow? headlessWindow_;

  private static void InitIfNull_() {
    if (headlessWindow_ != null) {
      return;
    }

    GLFWProvider.CheckForMainThread = false;
    var headlessWindowSettings = new NativeWindowSettings {
        StartVisible = false,
        RedBits = 8,
        BlueBits = 8,
        GreenBits = 8,
        AlphaBits = 8,
        DepthBits = 32,
    };
    
    if (GlConstants.Debug) {
      headlessWindowSettings.Flags = ContextFlags.Debug;
    }

    headlessWindow_ = new GameWindow(
        GameWindowSettings.Default,
        headlessWindowSettings);
  }

  public static void MakeCurrent() {
    InitIfNull_();

    GlUtil.SwitchContext(headlessWindow_);
    headlessWindow_.MakeCurrent();
    GlUtil.InitGl();
  }
}