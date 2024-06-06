using System;

namespace fin.util.progress {
  public interface IValueFractionProgress<T> {
    float Progress { get; }
    T? Value { get; }

    void ReportProgress(float progress);
    void ReportCompletion(T value);

    event EventHandler<float> OnProgressChanged;
    event EventHandler<T> OnComplete;
  }
}