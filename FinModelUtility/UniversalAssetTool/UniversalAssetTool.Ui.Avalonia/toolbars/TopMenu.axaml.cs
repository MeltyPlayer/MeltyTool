using System.Threading.Tasks;

using Avalonia.Controls;
using Avalonia.Interactivity;

using fin.io.web;
using fin.util.io;

using uni.ui.avalonia.common.buttons;
using uni.ui.avalonia.settings;

namespace uni.ui.avalonia.toolbars;

public partial class TopMenu : UserControl {
  public TopMenu() {
    InitializeComponent();
  }

  private void OpenFileWindowAndTryToImportAsset_(
      object? sender,
      RoutedEventArgs e)
    => Task.Run(() => ImportAssetButton
                    .OpenFileWindowAndTryToImportAsset(this));

  private void OpenSettingsWindow_(object? sender, RoutedEventArgs e) {
    var parentWindow = TopLevel.GetTopLevel(this) as Window;

    var settingsWindow = new SettingsWindow();
    if (parentWindow != null) {
      settingsWindow.Show(parentWindow);
    } else {
      settingsWindow.Show();
    }
  }

  private void OpenGithubInBrowser_(object? sender, RoutedEventArgs e)
    => WebBrowserUtil.OpenUrl(GitHubUtil.GITHUB_CHOOSE_NEW_ISSUE_URL);
}