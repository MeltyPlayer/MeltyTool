using Celeste64.api;

using fin.io;
using fin.io.bundles;
using fin.model.io.importers.gltf;
using fin.util.progress;

using fmod.api;

namespace uni.games.celeste_64;

public class Celeste64FileBundleGatherer : BPrereqsFileBundleGatherer {
  public override string Name => "celeste_64";

  protected override void GatherFileBundlesFromHierarchy(
      IFileBundleOrganizer organizer,
      IMutablePercentageProgress mutablePercentageProgress,
      IFileHierarchy fileHierarchy) {
    var root = fileHierarchy.Root;

    foreach (var bankFile in root.FilesWithExtensionRecursive(".bank")) {
      organizer.Add(new BankAudioFileBundle(bankFile).Annotate(bankFile));
    }

    var modelDirectory = root.AssertGetExistingSubdir("Models");
    foreach (var glbFile in
             modelDirectory.FilesWithExtensionRecursive(".glb")) {
      organizer.Add(new GltfModelFileBundle(glbFile).Annotate(glbFile));
    }

    var spritesDirectory = root.AssertGetExistingSubdir("Sprites");
    var textureDirectory = root.AssertGetExistingSubdir("Textures");
    foreach (var mapFile in root.AssertGetExistingSubdir("Maps")
                                .GetExistingFiles()) {
      organizer.Add(new Celeste64MapModelFileBundle {
          MapFile = mapFile,
          TextureDirectory = textureDirectory,
      }.Annotate(mapFile));
      organizer.Add(new Celeste64MapSceneFileBundle {
          MapFile = mapFile,
          ModelDirectory = modelDirectory,
          SpritesDirectory = spritesDirectory,
          TextureDirectory = textureDirectory,
      }.Annotate(mapFile));
    }
  }
}