using System;

using fin.util.progress;

namespace fin.io.bundles;

public sealed class FileBundleHandlerGatherer(
    Action<IFileBundleOrganizer, IMutablePercentageProgress> impl)
    : IFileBundleGatherer {
  public void GatherFileBundles(
      IFileBundleOrganizer organizer,
      IMutablePercentageProgress mutablePercentageProgress) {
    impl(organizer, mutablePercentageProgress);
    mutablePercentageProgress.ReportProgressAndCompletion();
  }
}

public sealed class FileBundleHandlerGathererWithoutProgress(
    Action<IFileBundleOrganizer> impl)
    : IFileBundleGatherer {
  public void GatherFileBundles(
      IFileBundleOrganizer organizer,
      IMutablePercentageProgress mutablePercentageProgress) {
    impl(organizer);
    mutablePercentageProgress.ReportProgressAndCompletion();
  }
}

public sealed class FileBundleHandlerGathererWithInput<T>(
    Action<IFileBundleOrganizer, IMutablePercentageProgress, T> impl,
    T input)
    : IFileBundleGatherer {
  public void GatherFileBundles(
      IFileBundleOrganizer organizer,
      IMutablePercentageProgress mutablePercentageProgress) {
    impl(organizer, mutablePercentageProgress, input);
    mutablePercentageProgress.ReportProgressAndCompletion();
  }
}

public sealed class FileBundleHandlerGathererWithoutProgressWithInput<T>(
    Action<IFileBundleOrganizer, T> impl,
    T input)
    : IFileBundleGatherer {
  public void GatherFileBundles(
      IFileBundleOrganizer organizer,
      IMutablePercentageProgress mutablePercentageProgress) {
    impl(organizer, input);
    mutablePercentageProgress.ReportProgressAndCompletion();
  }
}