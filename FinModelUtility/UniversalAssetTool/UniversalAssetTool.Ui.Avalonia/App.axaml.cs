using System.Linq;

using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;

using fin.ui.avalonia.styles;

using uni.games;
using uni.ui.avalonia.settings;
using uni.ui.avalonia.ViewModels;
using uni.ui.avalonia.Views;

namespace uni.ui.avalonia;

public partial class App : Application {
  public override void Initialize() {
    AvaloniaXamlLoader.Load(this);
    this.Styles.AddRange(new HeaderStyles());
  }

  public override void OnFrameworkInitializationCompleted() {
    if (this.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime
        desktop) {
      if (ExtractorUtil
          .GetExtractorsWhichNeedConfiguration()
          .Any(t => t.stillNeedsToBeConfigured)) {
        desktop.MainWindow = new FileBundleGathererSelectorWindow {
            DataContext = new FileBundleGathererSelectorWindowViewModel(),
            Desktop = desktop,
        };
      } else {
        desktop.MainWindow = new MainWindow {
            DataContext = new MainViewModel()
        };
      }
    } else if (this.ApplicationLifetime is ISingleViewApplicationLifetime
               singleViewPlatform) {
      singleViewPlatform.MainView = new MainView {
          DataContext = new MainViewModel()
      };
    }

    base.OnFrameworkInitializationCompleted();
  }
}