namespace fin.util.progress;

public static class SplitPercentageProgressExtensions {
  public static void ReportProgressAndCompletion(
      this IMutablePercentageProgress p) {
    p.ReportProgress(100);
    p.ReportCompletion();
  }

  public static IMutablePercentageProgress AsValueless<T>(
      this IMutablePercentageProgressValue<T> p) {
    var percentageProgress = new PercentageProgress();

    percentageProgress.OnProgressChanged
        += (_, progress) => p.ReportProgress(progress);

    return percentageProgress;
  }

  public static SplitPercentageProgress Split(this IMutablePercentageProgress p,
                                              int numBuckets) {
    var impl = new SplitPercentageProgress(numBuckets);

    impl.OnProgressChanged += (_, progress) => p.ReportProgress(progress);
    impl.OnComplete += (_, _) => p.ReportCompletion();

    return impl;
  }
}