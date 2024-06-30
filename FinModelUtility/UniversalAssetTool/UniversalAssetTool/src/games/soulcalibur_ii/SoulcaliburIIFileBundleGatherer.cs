using fin.io.bundles;
using fin.util.progress;

using uni.platforms.gcn;

namespace uni.games.soulcalibur_ii {
  public class SoulcaliburIIFileBundleGatherer : IAnnotatedFileBundleGatherer {
    public string Name => "soulcalibur_ii";

    public void GatherFileBundles(
        IFileBundleOrganizer organizer,
        IMutablePercentageProgress mutablePercentageProgress) {
      if (!new GcnFileHierarchyExtractor().TryToExtractFromGame(
              "soulcalibur_ii",
              out var fileHierarchy)) {
        return;
      }
    }
  }
}