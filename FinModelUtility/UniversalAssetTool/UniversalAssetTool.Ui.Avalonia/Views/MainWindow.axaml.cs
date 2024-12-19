using System;
using System.Threading.Tasks;

using Avalonia.Controls;
using Avalonia.Threading;
using ReactiveUI;

using fin.util.time;

using uni.ui.avalonia.common.dialogs;

namespace uni.ui.avalonia.Views;

public partial class MainWindow : Window {
  private readonly TimedCallback fpsCallback_;

  public MainWindow() {
    this.InitializeComponent();

    this.Closed += (_, _) => this.fpsCallback_.Dispose();

    this.fpsCallback_ = TimedCallback.WithPeriod(
        () => {
          Dispatcher.UIThread.Invoke(
              () => this.Title = FrameTime.FpsString);
        },
        .25f);

    ExceptionService.OnException += (e, c) => {
      var dialog = new ExceptionDialog {
          DataContext = new ExceptionDialogViewModel { Exception = e, Context = c},
          CanResize = false,
      };

      dialog.ShowDialog(this);
    };
  }
}