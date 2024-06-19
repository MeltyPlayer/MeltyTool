using Avalonia.Controls;
using Avalonia.Threading;

using fin.util.time;

namespace uni.ui.avalonia.Views;

public partial class MainWindow : Window {
  private readonly TimedCallback fpsCallback_;

  public MainWindow() {
    InitializeComponent();

    this.Closed += (_, _) => this.fpsCallback_.Dispose();

    this.fpsCallback_ = TimedCallback.WithPeriod(
        () => {
          Dispatcher.UIThread.Invoke(
              () => {
                var fps = FrameTime.SmoothedFps;
                this.Title = $"Universal Asset Tool ({fps:0.0} fps)";
              });
        },
        .25f);
  }
}