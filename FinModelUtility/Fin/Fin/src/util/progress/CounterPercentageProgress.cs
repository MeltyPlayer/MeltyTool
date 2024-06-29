using System;

namespace fin.util.progress;

public class CounterPercentageProgress(int total) : IPercentageProgress {
  private bool isComplete_;

  private int current_;

  public void Increment() {
    if (this.isComplete_) {
      return;
    }

    var current = this.current_++;
    if (current >= total) {
      this.OnComplete?.Invoke(this, EventArgs.Empty);
      return;
    }

    this.Progress = (100f * current) / total;
    this.OnProgressChanged?.Invoke(this, this.Progress);
  }

  public float Progress { get; private set; }
  public event EventHandler<float>? OnProgressChanged;
  public event EventHandler? OnComplete;
}