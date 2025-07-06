using fin.io;
using fin.io.bundles;
using fin.util.progress;

using uni.platforms.ds;

namespace uni.games;

public abstract class BDsFileBundleGatherer
    : INamedAnnotatedFileBundleGatherer {
  public abstract string Name { get; }

  protected abstract void GatherFileBundlesFromHierarchy(
      IFileBundleOrganizer organizer,
      IMutablePercentageProgress mutablePercentageProgress,
      IFileHierarchy fileHierarchy);

  public void GatherFileBundles(
      IFileBundleOrganizer organizer,
      IMutablePercentageProgress mutablePercentageProgress) {
    if (!new DsFileHierarchyExtractor().TryToExtractFromGame(
            this.Name,
            out var fileHierarchy)) {
      return;
    }

    this.GatherFileBundlesFromHierarchy(organizer,
                                        mutablePercentageProgress,
                                        fileHierarchy);
  }
}