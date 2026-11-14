using fin.util.tasks;

using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;


namespace fin.ui.rendering.gl;

public static class HeadlessGl {
  private static readonly DedicatedBackgroundWorker BACKGROUND_WORKER_ = new();
  private static GameWindow? headlessWindow_;

  public static Task InvokeOnHeadlessGlThread(Action handler) {
    return BACKGROUND_WORKER_.EnqueueWorkAsync(() => {
      try {
        MakeCurrent_();
        handler();
      } finally {
        MakeNoneCurrent_();
      }

      return BACKGROUND_WORKER_;
    });
  }

  public static Task<T> InvokeOnHeadlessGlThread<T>(Func<T> handler)
      where T : class {
    return BACKGROUND_WORKER_.EnqueueWorkAsync(() => {
      try {
        MakeCurrent_();
        return handler();
      } finally {
        MakeNoneCurrent_();
      }
    });
  }

  private static void MakeCurrent_() {
    InitIfNull_();

    GlUtil.SwitchContext(headlessWindow_?.Context);
    GlUtil.InitGl();
  }

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

  private static void MakeNoneCurrent_()
    => headlessWindow_?.Context.MakeNoneCurrent();
}