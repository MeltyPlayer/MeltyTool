using System;

namespace fin.util.progress;

public interface IIndeterminateProgress {
  event EventHandler OnComplete;
}

public interface IMutableIndeterminateProgress : IIndeterminateProgress {
  void ReportCompletion();
}

public interface IIndeterminateProgressValue<T> : IIndeterminateProgress {
  T? Value { get; }
  event EventHandler<T> OnCompleteValue;
}

public interface IMutableIndeterminateProgressValue<T>
    : IIndeterminateProgressValue<T> {
  void ReportCompletion(T value);
}

public interface IPercentageProgress : IIndeterminateProgress {
  float Progress { get; }
  event EventHandler<float> OnProgressChanged;
}

public interface IMutablePercentageProgress
    : IPercentageProgress, IMutableIndeterminateProgress {
  void ReportProgress(float progress);
}

public interface IPercentageProgressValue<T>
    : IIndeterminateProgressValue<T>, IPercentageProgress;

public interface IMutablePercentageProgressValue<T>
    : IPercentageProgressValue<T>;