using fin.common;
using fin.io;
using fin.io.bundles;
using fin.util.progress;

namespace uni.games;

public abstract class BPrereqsFileBundleGatherer
    : INamedAnnotatedFileBundleGatherer {
  public abstract string Name { get; }

  protected abstract void GatherFileBundlesFromHierarchy(
      IFileBundleOrganizer organizer,
      IMutablePercentageProgress mutablePercentageProgress,
      IFileHierarchy fileHierarchy);

  public void GatherFileBundles(
      IFileBundleOrganizer organizer,
      IMutablePercentageProgress mutablePercentageProgress) {
    if (!DirectoryConstants.ROMS_DIRECTORY.TryToGetExistingSubdir(
            Path.Join(this.Name, ExtractorUtil.PREREQS),
            out var prereqsDir)) {
      return;
    }

    var fileHierarchy = ExtractorUtil.GetFileHierarchy(this.Name, prereqsDir);
    this.GatherFileBundlesFromHierarchy(organizer,
                                        mutablePercentageProgress,
                                        fileHierarchy);
  }
}