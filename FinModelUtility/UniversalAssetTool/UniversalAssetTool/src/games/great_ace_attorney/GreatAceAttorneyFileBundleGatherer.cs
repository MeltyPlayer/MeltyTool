using fin.io.bundles;
using fin.util.progress;

using uni.platforms.threeDs;

namespace uni.games.great_ace_attorney {
  public class GreatAceAttorneyFileBundleGatherer
      : IAnnotatedFileBundleGatherer {
    public IEnumerable<IAnnotatedFileBundle> GatherFileBundles(
        IMutablePercentageProgress mutablePercentageProgress) {
      if (!new ThreeDsFileHierarchyExtractor().TryToExtractFromGame(
              "great_ace_attorney",
              out var fileHierarchy)) {
        yield break;
      }

      yield break;
    }
  }
}