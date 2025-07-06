using fin.io;
using fin.io.bundles;
using fin.util.progress;

using uni.platforms.gcn;

namespace uni.games;

public abstract class BGameCubeFileBundleGatherer
    : INamedAnnotatedFileBundleGatherer {
  public abstract string Name { get; }

  public virtual GcnFileHierarchyExtractor.Options Options
    => GcnFileHierarchyExtractor.Options.Standard();

  protected abstract void GatherFileBundlesFromHierarchy(
      IFileBundleOrganizer organizer,
      IMutablePercentageProgress mutablePercentageProgress,
      IFileHierarchy fileHierarchy);

  public void GatherFileBundles(
      IFileBundleOrganizer organizer,
      IMutablePercentageProgress mutablePercentageProgress) {
    if (!new GcnFileHierarchyExtractor().TryToExtractFromGame(
            this.Name,
            this.Options,
            out var fileHierarchy)) {
      return;
    }

    this.GatherFileBundlesFromHierarchy(organizer,
                                        mutablePercentageProgress,
                                        fileHierarchy);
  }
}