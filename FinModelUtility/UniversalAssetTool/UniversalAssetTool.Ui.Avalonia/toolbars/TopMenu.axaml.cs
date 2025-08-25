using System.Linq;
using System.Threading.Tasks;

using Avalonia.Controls;
using Avalonia.Interactivity;

using fin.io.web;
using fin.ui.avalonia;
using fin.util.io;

using ReactiveUI;

using uni.ui.avalonia.common.buttons;
using uni.ui.avalonia.io;
using uni.ui.avalonia.settings;
using uni.ui.avalonia.util;
using uni.ui.winforms.common.fileTreeView;

namespace uni.ui.avalonia.toolbars;

public class TopMenuModelForDesigner : TopMenuModel {
  public IFileTreeParentNode? SelectedDirectory => null;
}

public class TopMenuModel : ViewModelBase {
  public TopMenuModel() {
    this.SelectedDirectory = null;
    SelectedFileTreeDirectoryService.OnFileTreeDirectorySelected
        += directory => this.SelectedDirectory = directory;
  }

  public IFileTreeParentNode? SelectedDirectory {
    get;
    set {
      this.RaiseAndSetIfChanged(ref field, value);

      var fileBundleCount = value?.GetFiles(true).Count() ?? 0;
      this.ExportInDirectoryButtonEnabled = fileBundleCount > 0;
      this.ExportInDirectoryText
          = $"Export _all {fileBundleCount} asset{(fileBundleCount == 1 ? "" : "s")} in {value?.GetLocalPath() ?? "selected directory"} to out/";
    }
  }

  public bool ExportInDirectoryButtonEnabled {
    get;
    set => this.RaiseAndSetIfChanged(ref field, value);
  }

  public string ExportInDirectoryText {
    get;
    set => this.RaiseAndSetIfChanged(ref field, value);
  }
}

public partial class TopMenu : UserControl {
  public TopMenu() {
    InitializeComponent();
    this.DataContext = new TopMenuModel();
  }

  private void OpenFileWindowAndTryToImportAsset_(
      object? sender,
      RoutedEventArgs e)
    => Task.Run(() => ImportAssetButton
                    .OpenFileWindowAndTryToImportAsset(this));

  private void OpenSettingsWindow_(object? sender, RoutedEventArgs e)
    => this.ShowNewWindow(() => new SettingsWindow());

  private void OpenGithubInBrowser_(object? sender, RoutedEventArgs e)
    => WebBrowserUtil.OpenUrl(GitHubUtil.GITHUB_CHOOSE_NEW_ISSUE_URL);

  private void OpenExtractionProgressWindow_(object? sender,
                                             RoutedEventArgs e)
    => FileBundleGatherersService.ShowExtractorProgressWindow(this);
}