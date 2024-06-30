using fin.io.bundles;
using fin.util.progress;

using uni.platforms.gcn;

namespace uni.games.animal_crossing {
  public class AnimalCrossingFileBundleGatherer : IAnnotatedFileBundleGatherer {
    public void GatherFileBundles(
        IFileBundleOrganizer organizer,
        IMutablePercentageProgress mutablePercentageProgress) {
      if (!new GcnFileHierarchyExtractor().TryToExtractFromGame(
              "animal_crossing",
              out var fileHierarchy)) {
        return;
      }
    }
  }
}