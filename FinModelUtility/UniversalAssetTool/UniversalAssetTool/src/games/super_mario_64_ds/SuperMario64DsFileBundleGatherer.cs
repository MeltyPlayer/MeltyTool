using fin.common;
using fin.io.bundles;
using fin.util.progress;

using sm64ds.api;

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

    foreach (var directory in fileHierarchy) {
      var bmdFiles = directory.GetFilesWithFileType(".bmd").ToArray();
      if (bmdFiles.Length == 0) {
        continue;
      }

      if (bmdFiles.Length == 1) {
        var bmdFile = bmdFiles[0];
        var bcaFiles = directory.GetFilesWithFileType(".bca").ToArray();
        organizer.Add(new Sm64dsModelFileBundle {
            GameName = "super_mario_64_ds",
            BmdFile = bmdFile,
            BcaFiles = bcaFiles,
        }.Annotate(bmdFile));
      } else {
        foreach (var bmdFile in bmdFiles) {
          organizer.Add(new Sm64dsModelFileBundle {
              GameName = "super_mario_64_ds",
              BmdFile = bmdFile,
          }.Annotate(bmdFile));
        }
      }
    }
  }
}