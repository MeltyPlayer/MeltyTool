using bar.api;

using fin.archives;
using fin.io;
using fin.io.bundles;
using fin.util.progress;

namespace uni.games.beetle_adventure_racing;

public sealed class BeetleAdventureRacingFileBundleGatherer
    : BN64FileBundleGatherer {
  public override string Name => "beetle_adventure_racing";

  protected override void ExtractFilesFromRom(
      IReadOnlyTreeFile romFile,
      ISystemDirectory extractedDir,
      ISystemDirectory prereqsDir) {
    new BarFileTableImporter().ExtractInto(romFile, extractedDir);
  }

  protected override void GatherFileBundlesFromHierarchy(
      IFileBundleOrganizer organizer,
      IMutablePercentageProgress
          mutablePercentageProgress,
      IFileHierarchy fileHierarchy) {
    var rootDirectoryImpl = fileHierarchy.Root.Impl;

    foreach (var uvmdFile in
             fileHierarchy.Root.FilesWithExtensionRecursive(".uvmd")) {
      organizer.Add(new UvmdModelFileBundle(uvmdFile.Impl, rootDirectoryImpl));
    }

    foreach (var uvctFile in
             fileHierarchy.Root.FilesWithExtensionRecursive(".uvct")) {
      var uvctFileImpl = uvctFile.Impl;

      organizer.Add(new UvctModelFileBundle(uvctFileImpl, rootDirectoryImpl));
      organizer.Add(new UvctSceneFileBundle(uvctFileImpl, rootDirectoryImpl));
    }

    foreach (var uvtrFile in
             fileHierarchy.Root.FilesWithExtensionRecursive(".uvtr")) {
      var displayName = int.Parse(uvtrFile.NameWithoutExtension) switch {
          19 => "Coventry Cove",
          20 => "Sunset Sands",
          22 => "Inferno Isle",
          23 => "Wicked Woods",
          24 => "Airport",
          26 => "Castle",
          27 => "Stadium",
          28 => "Volcano",
          29 => "Dunes",
          30 => "Rooftops",
          31 => "Ice Flows",
          32 => "Parkade",
          33 => "Woods",
          _  => null,
      };

      organizer.Add(
          new UvtrSceneFileBundle(uvtrFile.Impl,
                                  rootDirectoryImpl,
                                  displayName));
    }

    var barSceneUvmoFile
        = fileHierarchy.Root.AssertGetExistingFile($"uvmo/{0x32}.uvmo").Impl;
    foreach (var (sceneIndex, name) in new[] {
                 (0x5, "Coventry Cove"), 
                 (0x6, "Wicked Woods"),
                 (0x7, "Mount Mayhem"),
                 (0x8, "Sunset Sands"),
                 (0x9, "Inferno Isle"),
                 (0xA, "Metro Madness"),
                 (0xB, "Stunt O'Rama"),
                 (0x11, "Airport"),
                 (0x12, "Castle"),
                 (0x13, "Stadium"),
                 (0x14, "Volcano"),
                 (0x15, "Dunes"),
                 (0x16, "Rooftops"),
                 (0x17, "Ice Flows"),
                 (0x18, "Parkade"),
                 (0x19, "Woods"),
             }) {
      organizer.Add(new BarSceneFileBundle(barSceneUvmoFile,
                                           rootDirectoryImpl,
                                           sceneIndex,
                                           name));
    }
  }
}