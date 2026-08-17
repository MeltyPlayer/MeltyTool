using fin.io;
using fin.io.bundles;
using fin.util.progress;

using tlpe.api;


namespace uni.games.the_labyrinth_plus_edition;

public sealed class TheLabyrinthPlusEditionFileBundleGatherer
    : BPrereqsFileBundleGatherer {
  public override string Name => "the_labyrinth_plus_edition";

  public override FileBundleGathererPlatform Platform
    => FileBundleGathererPlatform.DESKTOP;

  protected override void GatherFileBundlesFromHierarchy(
      IFileBundleOrganizer organizer,
      IMutablePercentageProgress mutablePercentageProgress,
      IFileHierarchy fileHierarchy) {
    var ballsFile = fileHierarchy.Root.AssertGetExistingFile("balls.gam").Impl;
    var texturesDir = fileHierarchy.Root.AssertGetExistingSubdir("Textures").Impl;

    foreach (var scbFile in fileHierarchy.Root.GetFilesWithFileType(".scb", true)) {
      organizer.Add(new ScbModelFileBundle(scbFile.Impl, ballsFile, texturesDir));
    }
  }
}