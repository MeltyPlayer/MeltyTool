using fin.common;
using fin.io.bundles;
using fin.util.progress;

namespace uni.games.rolling_madness;

public class RollingMadnessFileBundleGatherer : IAnnotatedFileBundleGatherer {
  public void GatherFileBundles(
      IFileBundleOrganizer organizer,
      IMutablePercentageProgress mutablePercentageProgress) {
    if (!DirectoryConstants.ROMS_DIRECTORY.TryToGetExistingSubdir(
            Path.Join("rolling_madness", ExtractorUtil.PREREQS),
            out var rmDir)) {
      return;
    }

    var fileHierarchy
        = ExtractorUtil.GetFileHierarchy("rolling_madness", rmDir);

    // TODO: Ase files probably use zlib compression, but how???
  }
}