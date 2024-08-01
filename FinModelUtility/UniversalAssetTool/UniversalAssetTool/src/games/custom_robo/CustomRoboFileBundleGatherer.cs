using fin.io.bundles;
using fin.util.progress;

using uni.platforms.gcn;

namespace uni.games.custom_robo;

public class CustomRoboFileBundleGatherer : IAnnotatedFileBundleGatherer {
  public void GatherFileBundles(
      IFileBundleOrganizer organizer,
      IMutablePercentageProgress mutablePercentageProgress) {
    if (!new GcnFileHierarchyExtractor().TryToExtractFromGame(
            "custom_robo",
            out var fileHierarchy)) {
      return;
    }
  }
}