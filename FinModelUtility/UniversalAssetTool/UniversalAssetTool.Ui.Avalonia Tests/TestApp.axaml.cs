using Avalonia;
using Avalonia.Markup.Xaml;

namespace uni.ui.avalonia;

public class TestApp : Application {
  public override void Initialize() => AvaloniaXamlLoader.Load(this);
}