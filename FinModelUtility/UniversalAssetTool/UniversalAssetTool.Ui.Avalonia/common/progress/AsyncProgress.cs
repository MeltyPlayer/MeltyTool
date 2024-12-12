using System;
using System.Threading.Tasks;

using fin.util.progress;

using ReactiveUI;

using uni.ui.avalonia.ViewModels;

namespace uni.ui.avalonia.common.progress;

public class AsyncProgress
    : ViewModelBase, IMutableIndeterminateProgressValue<object> {
  private bool isComplete_;
  private object? value_;

  public static AsyncProgress FromResult<T>(T t) {
    var progress = new AsyncProgress();
    progress.ReportCompletion(t!);
    return progress;
  }

  public static AsyncProgress FromTask<T>(Task<T> t) {
    var progress = new AsyncProgress();
    if (t.IsCompleted) {
      progress.ReportCompletion(t.Result!);
    } else {
      t.ContinueWith(v => { progress.ReportCompletion(v.Result!); });
    }

    return progress;
  }

  public bool IsComplete {
    get => this.isComplete_;
    private set => this.isComplete_ = value;
  }

  public object? Value {
    get => this.value_;
    private set => this.value_ = value;
  }

  public void ReportCompletion(object value) {
    if (this.IsComplete) {
      return;
    }

    this.OnCompleteValue?.Invoke(this, value);
    this.OnComplete?.Invoke(this, EventArgs.Empty);

    this.RaiseAndSetIfChanged(ref this.isComplete_,
                              true,
                              nameof(this.IsComplete));
    this.RaiseAndSetIfChanged(ref this.value_,
                              value,
                              nameof(this.Value));
  }

  public event EventHandler<object>? OnCompleteValue;
  public event EventHandler? OnComplete;
}