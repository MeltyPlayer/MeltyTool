using fin.io;
using fin.io.bundles;
using fin.util.progress;

using uni.platforms.wii;

namespace uni.games;

public abstract class BWiiFileBundleGatherer
    : INamedFileBundleGatherer {
  public abstract string Name { get; }
  public FileBundleGathererPlatform Platform => FileBundleGathererPlatform.WII;

  public bool IsAvailable
    => WiiFileHierarchyExtractor.TryToFindRom(this.Name, out _);

  protected abstract void GatherFileBundlesFromHierarchy(
      IFileBundleOrganizer organizer,
      IMutablePercentageProgress mutablePercentageProgress,
      IFileHierarchy fileHierarchy);

  public void GatherFileBundles(
      IFileBundleOrganizer organizer,
      IMutablePercentageProgress mutablePercentageProgress) {
    if (!new WiiFileHierarchyExtractor().TryToExtractFromGame(
            this.Name,
            out var fileHierarchy)) {
      return;
    }

    this.GatherFileBundlesFromHierarchy(organizer,
                                        mutablePercentageProgress,
                                        fileHierarchy);
  }
}