using Avalonia.Controls;

using ConfigFactory;
using ConfigFactory.Models;

namespace uni.ui.avalonia.settings;

public partial class SettingsView : UserControl {
  public SettingsView() {
    InitializeComponent();
    if (this.ConfigPage.DataContext is ConfigPageModel model) {
      model.Append(new SettingsViewModel());
    }
  }
}