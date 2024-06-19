using System;
using System.Collections.ObjectModel;

using Avalonia;
using Avalonia.ReactiveUI;

namespace uni.ui.avalonia.desktop;

class Program
{
    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    [STAThread]
    public static void Main(string[] args) => BuildAvaloniaApp()
        .StartWithClassicDesktopLifetime(args);

    // Avalonia configuration, don't remove; also used by visual designer.
    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .With(new Win32PlatformOptions {
                RenderingMode = new Collection<Win32RenderingMode>
                    { Win32RenderingMode.Wgl }
            })
            .WithInterFont()
            .LogToTrace()
            .UseReactiveUI();
}
