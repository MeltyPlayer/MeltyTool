using System;

using fin.util.progress;

namespace fin.io.bundles;

public static class AnnotatedFileBundleGathererExtensions {
  public static void TryToGatherAndReportCompletion(
      this IAnnotatedFileBundleGatherer gatherer,
      IFileBundleOrganizer organizer,
      IMutablePercentageProgress splitProgress) {
    try {
      gatherer.GatherFileBundles(organizer, splitProgress);
    } catch (Exception e) {
      Console.Error.WriteLine(e.ToString());
    } finally {
      splitProgress.ReportProgressAndCompletion();
    }
  }
}