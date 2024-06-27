using fin.io.bundles;
using fin.model.io;

using uni.platforms.gcn;

namespace uni.games.soulcalibur_ii;

public class SoulcaliburIIFileBundleGatherer
    : IAnnotatedFileBundleGatherer<IModelFileBundle> {
  public string Name => "soulcalibur_ii";

  public IEnumerable<IAnnotatedFileBundle<IModelFileBundle>>?
      GatherFileBundles() {
      if (!new GcnFileHierarchyExtractor().TryToExtractFromGame(
              "soulcalibur_ii",
              out var fileHierarchy)) {
        yield break;
      }
    }
}