using fin.io.bundles;

using uni.platforms.gcn;

namespace uni.games.doshin_the_giant;

public class DoshinTheGiantFileBundleGatherer : IAnnotatedFileBundleGatherer {
  public IEnumerable<IAnnotatedFileBundle> GatherFileBundles() {
      if (!new GcnFileHierarchyExtractor().TryToExtractFromGame(
              "doshin_the_giant",
              out var fileHierarchy)) {
        yield break;
      }

      yield break;
    }
}