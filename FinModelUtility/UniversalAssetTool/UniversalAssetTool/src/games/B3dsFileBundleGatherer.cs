using fin.io;
using fin.io.bundles;
using fin.util.progress;

using uni.platforms.threeDs;

namespace uni.games;

public abstract class B3dsFileBundleGatherer
    : INamedAnnotatedFileBundleGatherer {
  public abstract string Name { get; }

  protected abstract void GatherFileBundlesFromHierarchy(
      IFileBundleOrganizer organizer,
      IMutablePercentageProgress mutablePercentageProgress,
      IFileHierarchy fileHierarchy);

  public void GatherFileBundles(
      IFileBundleOrganizer organizer,
      IMutablePercentageProgress mutablePercentageProgress) {
    if (!new ThreeDsFileHierarchyExtractor().TryToExtractFromGame(
            this.Name,
            out var fileHierarchy)) {
      return;
    }

    this.GatherFileBundlesFromHierarchy(organizer,
                                        mutablePercentageProgress,
                                        fileHierarchy);
  }
}