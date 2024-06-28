using System;

using fin.util.progress;

using ReactiveUI;

using uni.ui.avalonia.ViewModels;

namespace uni.ui.avalonia.common.progress;

public class AsyncProgress : ViewModelBase, IAsyncProgress<object> {
  public object? Value { get; private set; }

  public void ReportCompletion(object value) {
    this.Value = value;
    this.OnComplete?.Invoke(this, this.Value);
    this.RaisePropertyChanged(nameof(this.Value));
  }

  public event EventHandler<object>? OnComplete;
}