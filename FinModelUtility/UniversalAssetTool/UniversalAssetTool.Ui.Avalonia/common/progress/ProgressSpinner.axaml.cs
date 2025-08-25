using System;
using System.Threading.Tasks;

using Avalonia.Controls;

using fin.ui.avalonia;

using ReactiveUI;

namespace uni.ui.avalonia.common.progress;

public class ProgressSpinnerViewModelForDesigner
    : ProgressSpinnerViewModel {
  public ProgressSpinnerViewModelForDesigner() {
    this.Progress = new ValueFractionProgress();

    var secondsToWait = 3;
    var start = DateTime.Now;

    Task.Run(
        async () => {
          DateTime current;
          double elapsedSeconds;
          do {
            current = DateTime.Now;
            elapsedSeconds = (current - start).TotalSeconds;
            this.Progress.ReportProgress(
                100 *
                Math.Clamp((float) (elapsedSeconds / secondsToWait), 0, 1));

            await Task.Delay(50);
          } while (elapsedSeconds < secondsToWait);

          this.Progress.ReportCompletion("Hello world!");
        });
  }
}

public class ProgressSpinnerViewModel : ViewModelBase {
  public ValueFractionProgress Progress {
    get;
    set => this.RaiseAndSetIfChanged(ref field, value);
  }
}

public partial class ProgressSpinner : UserControl {
  public ProgressSpinner() {
    this.InitializeComponent();
  }
}