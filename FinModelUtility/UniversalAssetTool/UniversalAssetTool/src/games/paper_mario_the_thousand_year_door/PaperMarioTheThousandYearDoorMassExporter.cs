using fin.io.bundles;
using fin.model.io;

using uni.platforms.gcn;

namespace uni.games.paper_mario_the_thousand_year_door {
  using IAnnotatedCmbBundle = IAnnotatedFileBundle<IModelFileBundle>;

  public class PaperMarioTheThousandYearDoorFileBundleGatherer
      : IAnnotatedFileBundleGatherer<IModelFileBundle> {
    public IEnumerable<IAnnotatedCmbBundle> GatherFileBundles() {
      if (!new GcnFileHierarchyExtractor().TryToExtractFromGame(
              "paper_mario_the_thousand_year_door",
              out var fileHierarchy)) {
        return Enumerable.Empty<IAnnotatedCmbBundle>();
      }

      return Enumerable.Empty<IAnnotatedCmbBundle>();
    }
  }
}