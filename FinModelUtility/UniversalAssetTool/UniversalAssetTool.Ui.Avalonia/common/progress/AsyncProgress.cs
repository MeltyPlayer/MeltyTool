using System;
using System.Threading.Tasks;

using fin.util.progress;

using ReactiveUI;

using uni.ui.avalonia.ViewModels;

namespace uni.ui.avalonia.common.progress;

public class AsyncProgress
    : ViewModelBase, IMutableIndeterminateProgressValue<object> {
  private bool isComplete_;

  public static AsyncProgress FromResult<T>(T t) {
    var progress = new AsyncProgress();
    progress.ReportCompletion(t!);
    return progress;
  }

  public static AsyncProgress FromTask<T>(Task<T> t) {
    var progress = new AsyncProgress();
    Task.Run(() => {
      t.Wait();
      progress.ReportCompletion(t.Result!);
    });
    return progress;
  }

  public object? Value { get; private set; }

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

  public event EventHandler<object>? OnCompleteValue;
  public event EventHandler? OnComplete;
}