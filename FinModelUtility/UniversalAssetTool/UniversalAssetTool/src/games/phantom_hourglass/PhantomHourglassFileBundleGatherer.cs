using fin.io.bundles;
using fin.util.progress;

using SceneGate.Ekona.Containers.Rom;

using uni.platforms;
using uni.platforms.ds;

using Yarhl.FileSystem;

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