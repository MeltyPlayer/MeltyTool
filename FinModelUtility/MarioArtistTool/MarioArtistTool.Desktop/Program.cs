using System;

using Avalonia;
using Avalonia.OpenGL;
using Avalonia.ReactiveUI;
using Avalonia.Win32;

using fin.services;
using fin.ui.avalonia;

namespace marioartisttool.desktop;

class Program {
  // Initialization code. Don't use any Avalonia, third-party APIs or any
  // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
  // yet and stuff might break.
  [STAThread]
  public static void Main(string[] args) {
    try {
      BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
    } catch (Exception e) {
      ExceptionService.HandleException(e, null);
    }
  }

  // Avalonia configuration, don't remove; also used by visual designer.
  public static AppBuilder BuildAvaloniaApp()
    => AppBuilderUtil.CreateFor<App>();
}