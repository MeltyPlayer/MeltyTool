using Avalonia.Controls;
using Avalonia.Interactivity;

using uni.ui.avalonia.settings;

namespace uni.ui.avalonia.common.buttons;

public partial class OpenSettingsButton : UserControl {
  public OpenSettingsButton() => this.InitializeComponent();

  private void Button_OnClick(object? sender, RoutedEventArgs e)
    => new SettingsWindow().Activate();
}