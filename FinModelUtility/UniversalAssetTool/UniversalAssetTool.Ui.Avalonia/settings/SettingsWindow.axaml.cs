using Avalonia.Controls;

using ConfigFactory;
using ConfigFactory.Models;

namespace uni.ui.avalonia.settings;

public partial class SettingsWindow : Window {
  static SettingsWindow() {
    EnumCheckboxesControlBuilder.Shared.Register();
  }

  public SettingsWindow() {
    this.InitializeComponent();

    if (this.ConfigPage.DataContext is ConfigPageModel model) {
      var settingsViewModel = new SettingsViewModel();
      model.Append(settingsViewModel);

      settingsViewModel.OnClose += this.Close;
    }
  }
}