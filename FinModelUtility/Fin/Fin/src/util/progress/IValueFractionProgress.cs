using System;

namespace fin.util.progress;

public interface IAsyncProgress<T> {
  T? Value { get; }
  void ReportCompletion(T value);
  event EventHandler<T> OnComplete;
}

public interface IValueFractionProgress<T> : IAsyncProgress<T> {
  float Progress { get; }
  void ReportProgress(float progress);
  event EventHandler<float> OnProgressChanged;
}