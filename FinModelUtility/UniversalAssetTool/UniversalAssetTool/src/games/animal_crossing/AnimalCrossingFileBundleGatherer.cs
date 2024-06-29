using fin.io.bundles;
using fin.util.progress;

using uni.platforms.gcn;

namespace uni.games.animal_crossing {
  public class AnimalCrossingFileBundleGatherer : IAnnotatedFileBundleGatherer {
    public IEnumerable<IAnnotatedFileBundle> GatherFileBundles(
        IMutablePercentageProgress mutablePercentageProgress) {
      if (!new GcnFileHierarchyExtractor().TryToExtractFromGame(
              "animal_crossing",
              out var fileHierarchy)) {
        yield break;
      }
    }
  }
}