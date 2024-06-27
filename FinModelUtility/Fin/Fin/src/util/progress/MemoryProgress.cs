using System;

namespace fin.util.progress;

public class MemoryProgress<T> : IProgress<T> {
  private readonly Progress<T> impl_;
  private IProgress<T> Impl => this.impl_;

  public MemoryProgress(T initial) {
    this.impl_ = new Progress<T>();
    this.impl_.ProgressChanged += (_, value) => this.Current = value;

    this.Impl.Report(initial);
  }

  public T Current { get; private set; }
  public void Report(T value) => this.Impl.Report(value);

  public event EventHandler<T> ProgressChanged {
    add {
      this.impl_.ProgressChanged += value;
      value.Invoke(null, Current);
    }
    remove => this.impl_.ProgressChanged -= value;
  }
}