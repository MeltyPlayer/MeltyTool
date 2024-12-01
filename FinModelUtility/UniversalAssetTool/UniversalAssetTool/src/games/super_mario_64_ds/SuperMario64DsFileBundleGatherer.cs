using fin.io.bundles;
using fin.util.progress;

using sm64ds.api;

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

    var fileHierarchy
        = new DsFileHierarchyExtractor().ExtractFromRom(superMario64DsRom);

    foreach (var bmdFile in
             fileHierarchy.Root.FilesWithExtensionRecursive(".bmd")) {
      organizer.Add(new Sm64dsModelFileBundle {
          GameName = "super_mario_64_ds", BmdFile = bmdFile
      }.Annotate(bmdFile));
    }
  }
}