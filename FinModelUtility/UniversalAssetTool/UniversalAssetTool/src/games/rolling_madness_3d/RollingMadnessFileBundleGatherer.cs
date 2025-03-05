using fin.common;
using fin.io.bundles;
using fin.util.progress;

using rollingMadness.api;

namespace uni.games.rolling_madness_3d;

public class RollingMadness3dFileBundleGatherer : IAnnotatedFileBundleGatherer {
  public void GatherFileBundles(
      IFileBundleOrganizer organizer,
      IMutablePercentageProgress mutablePercentageProgress) {
    if (!DirectoryConstants.ROMS_DIRECTORY.TryToGetExistingSubdir(
            Path.Join("rolling_madness_3d", ExtractorUtil.PREREQS),
            out var rmDir)) {
      return;
    }

    var fileHierarchy
        = ExtractorUtil.GetFileHierarchy("rolling_madness_3d", rmDir);

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