using System;

using Avalonia;
using Avalonia.OpenGL;
using Avalonia.ReactiveUI;

using uni.cli;

namespace uni.ui.avalonia.desktop;

class Program {
  // Initialization code. Don't use any Avalonia, third-party APIs or any
  // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
  // yet and stuff might break.
  [STAThread]
  public static void Main(string[] args) {
    Cli.Run(args,
            () => {
              try {
                BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
              } catch (Exception e) {
                ;
              }
            });
  }

  // Avalonia configuration, don't remove; also used by visual designer.
  public static AppBuilder BuildAvaloniaApp()
    => AppBuilder.Configure<App>()
                 .UsePlatformDetect()
                 .With(new Win32PlatformOptions {
                     // TODO: Locks framerate to 60fps
                     RenderingMode = [Win32RenderingMode.Wgl],
                     // TODO: Allows GL.Begin to work
                     WglProfiles
                         = [new GlVersion(GlProfileType.OpenGL, 3, 1)],
                 })
                 .WithInterFont()
                 .UseReactiveUI();
}