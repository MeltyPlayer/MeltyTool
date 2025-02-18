using fin.common;
using fin.io.bundles;
using fin.util.progress;

using uni.platforms.ds;

namespace uni.games.phantom_hourglass;

public class PhantomHourglassFileBundleGatherer
    : IAnnotatedFileBundleGatherer {
  public void GatherFileBundles(
      IFileBundleOrganizer organizer,
      IMutablePercentageProgress mutablePercentageProgress) {
    if (!DirectoryConstants.ROMS_DIRECTORY.TryToGetExistingFile(
            "phantom_hourglass.nds",
            out var phantomHourglassRom)) {
      return;
    }

    var fileHierarchy
        = new DsFileHierarchyExtractor().ExtractFromRom(phantomHourglassRom);
  }
}