using fin.io;
using fin.io.bundles;
using fin.util.progress;

using gdl.api;

namespace uni.games.gauntlet_dark_legacy;

public sealed class GauntletDarkLegacyFileBundleGatherer
    : BGameCubeFileBundleGatherer {
  public override string Name => "gauntlet_dark_legacy";
  public override string Title => "Gauntlet Dark Legacy";

  protected override void GatherFileBundlesFromHierarchy(
      IFileBundleOrganizer organizer,
      IMutablePercentageProgress mutablePercentageProgress,
      IFileHierarchy fileHierarchy) {
    var monstersDirectory =
        fileHierarchy.Root.AssertGetExistingSubdir("Gauntlet/MONSTERS");

    foreach (var directory in fileHierarchy) {
      if (!directory.TryToGetExistingFile("objects.ngc", out var objectsFile)) {
        continue;
      }

      if (!directory.TryToGetExistingFile("textures.ngc",
                                          out var texturesFile)) {
        continue;
      }

      if (!directory.TryToGetExistingFile("../anim/anim.ps2",
                                          out var animFile) &&
          !directory.TryToGetExistingFile("anim.ps2", out animFile)) {
        continue;
      }

      directory.TryToGetExistingFile("worlds.ps2", out var worldsFile);

      if (worldsFile == null) {
        organizer.Add(
            new GauntletDarkLegacyModelFileBundle {
                ObjectsFile = objectsFile.Impl,
                AnimFile = animFile.Impl,
                TexturesFile = texturesFile.Impl,
            });
      } else {
        organizer.Add(
            new GauntletDarkLegacyWorldModelFileBundle {
                ObjectsFile = objectsFile.Impl,
                AnimFile = animFile.Impl,
                TexturesFile = texturesFile.Impl,
                WorldsFile = worldsFile.Impl,
            });
        organizer.Add(
            new GauntletDarkLegacySceneFileBundle {
                ObjectsFile = objectsFile.Impl,
                AnimFile = animFile.Impl,
                TexturesFile = texturesFile.Impl,
                WorldsFile = worldsFile.Impl,
                MonstersDirectory = monstersDirectory.Impl,
            });
      }
    }
  }
}