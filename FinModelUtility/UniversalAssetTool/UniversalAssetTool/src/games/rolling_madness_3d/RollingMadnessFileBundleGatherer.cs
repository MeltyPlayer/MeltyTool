using fin.common;
using fin.io;
using fin.io.bundles;
using fin.util.progress;

using rollingMadness.api;

namespace uni.games.rolling_madness_3d;

public class RollingMadness3dFileBundleGatherer : BPrereqsFileBundleGatherer {
  public override string Name => "rolling_madness_3d";

  protected override void GatherFileBundlesFromHierarchy(
      IFileBundleOrganizer organizer,
      IMutablePercentageProgress mutablePercentageProgress,
      IFileHierarchy fileHierarchy) {
    var textureDirectory
        = fileHierarchy.Root.AssertGetExistingSubdir("texture");
    foreach (var aseMeshFile in fileHierarchy.Root.FilesWithExtensionRecursive(
                 ".ase.mesh")) {
      organizer.Add(
          new AseMeshModelFileBundle(aseMeshFile, textureDirectory).Annotate(
              aseMeshFile));
    }
  }
}