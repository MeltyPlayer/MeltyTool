using System;
using System.Threading.Tasks;

using fin.util.progress;

using ReactiveUI;

using uni.ui.avalonia.ViewModels;

namespace uni.ui.avalonia.common.progress;

public class ValueFractionProgress
    : ViewModelBase, IMutablePercentageProgressValue<object> {
  private bool isComplete_;

  public float Progress { get; private set; }
  public object? Value { get; private set; }

  public void ReportProgress(float progress1To100) {
    if (this.isComplete_) {
      return;
    }

    this.Progress = progress1To100;

    this.OnProgressChanged?.Invoke(this, this.Progress);
    this.RaisePropertyChanged(nameof(this.Progress));
  }

  public void ReportCompletion(object value) {
    if (this.isComplete_) {
      return;
    }

    this.isComplete_ = true;

    this.Value = value;

    this.OnCompleteValue?.Invoke(this, this.Value);
    this.OnComplete?.Invoke(this, EventArgs.Empty);

    this.RaisePropertyChanged(nameof(this.Value));
  }

  public event EventHandler<float>? OnProgressChanged;
  public event EventHandler<object>? OnCompleteValue;
  public event EventHandler? OnComplete;


  public static ValueFractionProgress FromTimer(
      float secondsToWait,
      object value) {
    var progress = new ValueFractionProgress();

    var start = DateTime.Now;

    Task.Run(async () => {
      DateTime current;
      double elapsedSeconds;
      do {
        current = DateTime.Now;
        elapsedSeconds = (current - start).TotalSeconds;
        progress.ReportProgress(
            100 *
            Math.Clamp((float) (elapsedSeconds / secondsToWait), 0, 1));

        await Task.Delay(50);
      } while (elapsedSeconds < secondsToWait);

      progress.ReportCompletion("Hello world!");
    });

    return progress;
  }
}