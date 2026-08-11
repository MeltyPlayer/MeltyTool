using System.Linq;

using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;

using uni.games;
using uni.ui.avalonia.settings;
using uni.ui.avalonia.ViewModels;
using uni.ui.avalonia.Views;

namespace uni.ui.avalonia;

public partial class App : Application {
  public override void Initialize() {
    AvaloniaXamlLoader.Load(this);
  }

  public override void OnFrameworkInitializationCompleted() {
    if (this.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime
        desktop) {
      if (ExtractorUtil
          .GetExtractorsWhichNeedConfiguration()
          .Any(t => t.stillNeedsToBeConfigured)) {
        desktop.MainWindow = new FileBundleGathererSelectorWindow {
            ViewModel = new FileBundleGathererSelectorWindowViewModel(),
            Desktop = desktop,
        };
      } else {
        desktop.MainWindow = new MainWindow {
            ViewModel = new MainViewModel()
        };
      }
    } else if (this.ApplicationLifetime is ISingleViewApplicationLifetime
               singleViewPlatform) {
      singleViewPlatform.MainView = new MainView {
          ViewModel = new MainViewModel()
      };
    }

    base.OnFrameworkInitializationCompleted();
  }
}