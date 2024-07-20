using fin.io.bundles;
using fin.util.progress;

using uni.platforms;
using uni.platforms.ds;

namespace uni.games.super_mario_64_ds;

public class SuperMario64DsFileBundleGatherer : IAnnotatedFileBundleGatherer {
  public void GatherFileBundles(
      IFileBundleOrganizer organizer,
      IMutablePercentageProgress mutablePercentageProgress) {
    if (!DirectoryConstants.ROMS_DIRECTORY.TryToGetExistingFile(
            "super_mario_64_ds.nds",
            out var superMario64DsRom)) {
      return;
    }

    new DsFileHierarchyExtractor().ExtractFromRom(superMario64DsRom);
  }
}