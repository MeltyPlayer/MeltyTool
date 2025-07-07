using System;
using System.Collections.Generic;
using System.Linq;

namespace fin.util.progress;

public class DelayedSplitPercentageProgress(int capacity = 0)
    : IPercentageProgress {
  private bool isComplete_;
  private readonly List<PercentageProgress> progresses_ = new(capacity);
  private readonly List<bool> eachIsComplete_ = new();

  public PercentageProgress this[int index] => this.progresses_[index];

  public IPercentageProgress Add() {
    var currentIndex = this.progresses_.Count;

    var progress = new PercentageProgress();
    this.progresses_.Add(progress);
    this.eachIsComplete_.Add(false);

    progress.OnProgressChanged += (_, _) => this.ReportProgressChanged_();
    progress.OnComplete += (_, _) => this.ReportComplete_(currentIndex);

    return progress;
  }

  private void ReportProgressChanged_() {
    if (this.isComplete_) {
      return;
    }

    var totalProgress = 0f;

    foreach (var progress in this.progresses_) {
      totalProgress += progress.Progress;
    }

    this.Progress = totalProgress / this.progresses_.Count;
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
}