using fin.common;
using fin.io.bundles;
using fin.util.progress;

using uni.platforms.ds;

namespace uni.games.mario_kart_ds;

public class MarioKartDsFileBundleGatherer : IAnnotatedFileBundleGatherer {
  public void GatherFileBundles(
      IFileBundleOrganizer organizer,
      IMutablePercentageProgress mutablePercentageProgress) {
    if (!DirectoryConstants.ROMS_DIRECTORY.TryToGetExistingFile(
            "mario_kart_ds.nds",
            out var marioKartDsRom)) {
      return;
    }

    var fileHierarchy
        = new DsFileHierarchyExtractor().ExtractFromRom(marioKartDsRom);
  }
}