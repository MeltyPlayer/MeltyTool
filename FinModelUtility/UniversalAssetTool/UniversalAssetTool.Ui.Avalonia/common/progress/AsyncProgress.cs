using System;

using fin.util.progress;

using ReactiveUI;

using uni.ui.avalonia.ViewModels;

namespace uni.ui.avalonia.common.progress;

public class AsyncProgress
    : ViewModelBase, IMutableIndeterminateProgressValue<object> {
  public object? Value { get; private set; }

  public void ReportCompletion(object value) {
    this.Value = value;
    this.OnCompleteValue?.Invoke(this, this.Value);
    this.OnComplete?.Invoke(this, EventArgs.Empty);
    this.RaisePropertyChanged(nameof(this.Value));
  }

  public event EventHandler<object>? OnCompleteValue;
  public event EventHandler? OnComplete;
}