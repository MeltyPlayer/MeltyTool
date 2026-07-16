using fin.common;
using fin.io;
using fin.io.bundles;
using fin.util.progress;

using marioartist.api;


namespace uni.games.mario_artist;

public sealed class MarioArtistFileBundleGatherer : BPrereqsFileBundleGatherer {
  public override string Name => "mario_artist";

  protected override void GatherFileBundlesFromHierarchy(
      IFileBundleOrganizer organizer,
      IMutablePercentageProgress mutablePercentageProgress,
      IFileHierarchy fileHierarchy) {
    var root = fileHierarchy.Root;

    DirectoryConstants.ROMS_DIRECTORY.TryToGetExistingFile("mario_artist_talent_studio.z64", out var romFile);

    var animationsDirectory
        = fileHierarchy.Root.AssertGetExistingSubdir("animations").Impl;

    foreach (var ma3d1File in root.FilesWithExtensionRecursive(".ma3d1")) {
      organizer.Add(new Ma3d1ModelFileBundle(ma3d1File.Impl));
    }

    foreach (var tstltFile in root.FilesWithExtensionRecursive(".tstlt")) {
      organizer.Add(new TstltModelFileBundle(tstltFile.Impl, animationsDirectory, romFile));
    }
  }
}