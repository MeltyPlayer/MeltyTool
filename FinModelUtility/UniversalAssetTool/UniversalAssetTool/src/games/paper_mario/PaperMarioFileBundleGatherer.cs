using fin.archives;
using fin.io;
using fin.io.bundles;
using fin.util.progress;

using pm.api;

namespace uni.games.paper_mario;

public sealed class PaperMarioFileBundleGatherer : BN64FileBundleGatherer {
  public override string Name => "paper_mario";

  protected override void ExtractFilesFromRom(
      IReadOnlyTreeFile romFile,
      ISystemDirectory extractedDir,
      ISystemDirectory prereqsDir)
    => new PaperMarioFileTableImporter().ExtractInto(
        new PaperMarioRomFileBundle(romFile),
        extractedDir);

  protected override void GatherFileBundlesFromHierarchy(
      IFileBundleOrganizer organizer,
      IMutablePercentageProgress mutablePercentageProgress,
      IFileHierarchy fileHierarchy) {
    var rootDir = fileHierarchy.Root;

    var assetsDir = rootDir.AssertGetExistingSubdir("assets").Impl;

    foreach (var areaDir in rootDir.AssertGetExistingSubdir("areas")
                                   .GetExistingSubdirs()) {
      var areaFile = areaDir.AssertGetExistingFile("area.json").Impl;

      foreach (var mapDir in areaDir.GetExistingSubdirs()) {
        var mapFile = mapDir.AssertGetExistingFile("map.json").Impl;
        var romOverlayFile
            = mapDir.AssertGetExistingFile("romOverlay.bin").Impl;

        organizer.Add(new PaperMarioMapSceneFileBundle(
                          areaFile,
                          mapFile,
                          romOverlayFile,
                          assetsDir));
      }
    }
  }
}