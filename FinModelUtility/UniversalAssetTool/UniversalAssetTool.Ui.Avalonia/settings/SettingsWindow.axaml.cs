using Avalonia.Controls;

using ConfigFactory;
using ConfigFactory.Models;

using uni.config;

namespace uni.ui.avalonia.settings;

public partial class SettingsWindow : Window {
  public SettingsWindow() {
    InitializeComponent();

    if (this.ConfigPage.DataContext is ConfigPageModel model) {
      var settingsViewModel = new SettingsViewModel();
      model.Append(settingsViewModel);

      settingsViewModel.OnClose += this.Close;
    }
  }
}