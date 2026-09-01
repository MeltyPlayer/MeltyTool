using fin.io;
using fin.io.bundles;
using fin.util.progress;

using uni.platforms;

namespace uni.games;

public abstract class BPs1FileBundleGatherer : INamedFileBundleGatherer {
  public abstract string Name { get; }
  public FileBundleGathererPlatform Platform => FileBundleGathererPlatform.PS1;

  public bool IsAvailable
    => DiscFileHierarchyExtractor.HasRomOrExtractedDirectory(this.Name);

  protected abstract void GatherFileBundlesFromHierarchy(
      IFileBundleOrganizer organizer,
      IMutablePercentageProgress mutablePercentageProgress,
      IFileHierarchy fileHierarchy);

  public void GatherFileBundles(
      IFileBundleOrganizer organizer,
      IMutablePercentageProgress mutablePercentageProgress) {
    if (!DiscFileHierarchyExtractor.TryToExtractFromGame(
            this.Name,
            out var fileHierarchy)) {
      return;
    }

    this.GatherFileBundlesFromHierarchy(
        organizer,
        mutablePercentageProgress,
        fileHierarchy);
  }
}