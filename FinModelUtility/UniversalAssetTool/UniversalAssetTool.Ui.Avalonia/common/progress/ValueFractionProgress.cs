using System;

using fin.math.floats;
using fin.util.progress;

using ReactiveUI;

using uni.ui.avalonia.ViewModels;

namespace uni.ui.avalonia.common.progress;

public class ValueFractionProgress
    : ViewModelBase, IMutablePercentageProgressValue<object> {
  public float Progress { get; private set; }
  public object? Value { get; private set; }

  public void ReportProgress(float progress1To100) {
    if (this.Progress.IsRoughly(progress1To100)) {
      return;
    }

    this.Progress = progress1To100;

    this.OnProgressChanged?.Invoke(this, this.Progress);
    this.RaisePropertyChanged(nameof(this.Progress));
  }

  public void ReportCompletion(object value) {
    this.Progress = 100;
    this.Value = value;

    this.OnProgressChanged?.Invoke(this, this.Progress);
    this.OnCompleteValue?.Invoke(this, this.Value);
    this.OnComplete?.Invoke(this, EventArgs.Empty);

    this.RaisePropertyChanged(nameof(this.Progress));
    this.RaisePropertyChanged(nameof(this.Value));
  }

  public event EventHandler<float>? OnProgressChanged;
  public event EventHandler<object>? OnCompleteValue;
  public event EventHandler? OnComplete;
}