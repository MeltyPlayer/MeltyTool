using System;
using System.Linq;

namespace fin.util.progress;

public class SplitPercentageProgress : IPercentageProgress {
  private bool isComplete_;
  private readonly PercentageProgress[] progresses_;
  private readonly bool[] eachIsComplete_;

  public SplitPercentageProgress(int numBuckets) {
    this.progresses_ = new PercentageProgress[numBuckets];
    this.eachIsComplete_ = new bool[numBuckets];

    for (var i = 0; i < numBuckets; i++) {
      var progress = this.progresses_[i] = new PercentageProgress();

      var currentIndex = i;

      progress.OnProgressChanged += (_, _) => this.ReportProgressChanged_();
      progress.OnComplete += (_, _) => this.ReportComplete_(currentIndex);
    }
  }

  private void ReportProgressChanged_() {
    if (this.isComplete_) {
      return;
    }

    var totalProgress = 0f;

    foreach (var progress in this.progresses_) {
      totalProgress += progress.Progress;
    }

    this.Progress = totalProgress / this.progresses_.Length;
    this.OnProgressChanged?.Invoke(this, this.Progress);
  }

  private void ReportComplete_(int index) {
    if (this.isComplete_) {
      return;
    }

    this.eachIsComplete_[index] = true;
    this.isComplete_ = this.eachIsComplete_.All(b => b);
    if (this.isComplete_) {
      this.OnComplete?.Invoke(this, EventArgs.Empty);
    }
  }

  public float Progress { get; private set; }
  public event EventHandler<float>? OnProgressChanged;
  public event EventHandler? OnComplete;

  public PercentageProgress this[int index] => this.progresses_[index];
}