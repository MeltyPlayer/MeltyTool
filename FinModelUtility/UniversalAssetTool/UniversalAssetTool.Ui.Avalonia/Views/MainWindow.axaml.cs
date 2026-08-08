using Avalonia.Threading;

using fin.config.avalonia.services;
using fin.services;
using fin.ui.avalonia.controls;
using fin.ui.avalonia.dialogs;
using fin.util.tasks;
using fin.util.time;

using uni.ui.avalonia.ViewModels;

namespace uni.ui.avalonia.Views;

public partial class MainWindow : BWindow<MainViewModel> {
  private readonly TimedCallback fpsCallback_;

  public MainWindow() {
    this.InitializeComponent();

    this.Closed += (_, _) => this.fpsCallback_.Dispose();

    this.fpsCallback_ = TimedCallback.WithPeriod(
        () => {
          try {
            Dispatcher.UIThread.Invoke(
                () => this.Title = FrameTime.FpsString);
          } catch {}
        },
        .25f);

    ExceptionService.OnException += (e, c) => {
      var dialog = new ExceptionDialog {
          DataContext = new ExceptionDialogViewModel { Exception = e, Context = c},
          CanResize = false,
      };

      dialog.ShowDialog(this);
    };

    TopLevelService.Init(this);
    FinTask.RunOnUiThread = Dispatcher.UIThread.Invoke;
  }
}