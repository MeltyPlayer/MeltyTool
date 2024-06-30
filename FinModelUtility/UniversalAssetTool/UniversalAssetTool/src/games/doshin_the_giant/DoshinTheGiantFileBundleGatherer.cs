using fin.io.bundles;
using fin.util.progress;

using uni.platforms.gcn;

namespace uni.games.doshin_the_giant {
  public class DoshinTheGiantFileBundleGatherer : IAnnotatedFileBundleGatherer {
    public void GatherFileBundles(
        IFileBundleOrganizer organizer,
        IMutablePercentageProgress mutablePercentageProgress) {
      if (!new GcnFileHierarchyExtractor().TryToExtractFromGame(
              "doshin_the_giant",
              out var fileHierarchy)) {
        return;
      }
    }
  }
}