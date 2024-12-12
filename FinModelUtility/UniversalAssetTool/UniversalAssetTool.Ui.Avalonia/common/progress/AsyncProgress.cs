using System;
using System.Threading.Tasks;

using fin.util.progress;

using ReactiveUI;

using uni.ui.avalonia.ViewModels;

namespace uni.ui.avalonia.common.progress;

public class AsyncProgress
    : ViewModelBase, IMutableIndeterminateProgressValue<object> {
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

  public bool IsComplete { get; private set; }
  public object? Value { get; private set; }

  public void ReportCompletion(object value) {
    if (this.IsComplete) {
      return;
    }

    this.IsComplete = true;

    this.Value = value;
    this.OnCompleteValue?.Invoke(this, this.Value);
    this.OnComplete?.Invoke(this, EventArgs.Empty);
    this.RaisePropertyChanged(nameof(this.Value));
  }

  public event EventHandler<object>? OnCompleteValue;
  public event EventHandler? OnComplete;
}