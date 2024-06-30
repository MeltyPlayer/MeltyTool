using System;

using fin.util.progress;

namespace fin.io.bundles;

public class AnnotatedFileBundleHandlerGatherer(
    Action<IFileBundleOrganizer, IMutablePercentageProgress> impl)
    : IAnnotatedFileBundleGatherer {
  public void GatherFileBundles(
      IFileBundleOrganizer organizer,
      IMutablePercentageProgress mutablePercentageProgress) {
    impl(organizer, mutablePercentageProgress);
    mutablePercentageProgress.ReportProgressAndCompletion();
  }
}

public class AnnotatedFileBundleHandlerGathererWithoutProgress(
    Action<IFileBundleOrganizer> impl)
    : IAnnotatedFileBundleGatherer {
  public void GatherFileBundles(
      IFileBundleOrganizer organizer,
      IMutablePercentageProgress mutablePercentageProgress) {
    impl(organizer);
    mutablePercentageProgress.ReportProgressAndCompletion();
  }
}

public class AnnotatedFileBundleHandlerGathererWithInput<T>(
    Action<IFileBundleOrganizer, IMutablePercentageProgress, T> impl,
    T input)
    : IAnnotatedFileBundleGatherer {
  public void GatherFileBundles(
      IFileBundleOrganizer organizer,
      IMutablePercentageProgress mutablePercentageProgress) {
    impl(organizer, mutablePercentageProgress, input);
    mutablePercentageProgress.ReportProgressAndCompletion();
  }
}

public class AnnotatedFileBundleHandlerGathererWithoutProgressWithInput<T>(
    Action<IFileBundleOrganizer, T> impl,
    T input)
    : IAnnotatedFileBundleGatherer {
  public void GatherFileBundles(
      IFileBundleOrganizer organizer,
      IMutablePercentageProgress mutablePercentageProgress) {
    impl(organizer, input);
    mutablePercentageProgress.ReportProgressAndCompletion();
  }
}