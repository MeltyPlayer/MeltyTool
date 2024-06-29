using System;

namespace fin.util.progress;

public class PercentageProgress : IMutablePercentageProgress {
  private bool isComplete_;
  public float Progress { get; private set; }

  public event EventHandler<float>? OnProgressChanged;
  public event EventHandler? OnComplete;

  public void ReportCompletion() {
    if (this.isComplete_) {
      return;
    }

    this.isComplete_ = true;
    this.OnComplete?.Invoke(this, EventArgs.Empty);
  }

  public void ReportProgress(float progress) {
    if (this.isComplete_) {
      return;
    }

    this.Progress = progress;
    this.OnProgressChanged?.Invoke(this, progress);
  }
}