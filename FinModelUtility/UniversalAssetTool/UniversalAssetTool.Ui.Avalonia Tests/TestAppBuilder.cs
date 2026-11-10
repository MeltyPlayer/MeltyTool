using Avalonia;
using Avalonia.Headless;

namespace uni.ui.avalonia;

public class TestAppBuilder {
  public static AppBuilder BuildAvaloniaApp() => AppBuilder.Configure<App>()
      .UseHeadless(new AvaloniaHeadlessPlatformOptions());
}