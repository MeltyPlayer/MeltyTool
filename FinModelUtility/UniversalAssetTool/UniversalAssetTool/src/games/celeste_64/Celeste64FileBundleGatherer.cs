using Celeste64;

using fin.common;
using fin.io.bundles;
using fin.model.io.importers.gltf;
using fin.util.progress;

namespace uni.games.celeste_64;

public class Celeste64FileBundleGatherer : IAnnotatedFileBundleGatherer {
  public void GatherFileBundles(
      IFileBundleOrganizer organizer,
      IMutablePercentageProgress mutablePercentageProgress) {
    if (!DirectoryConstants.ROMS_DIRECTORY.TryToGetExistingSubdir(
            Path.Join("celeste_64", ExtractorUtil.PREREQS),
            out var celeste64Dir)) {
      return;
    }

    var fileHierarchy
        = ExtractorUtil.GetFileHierarchy("celeste_64", celeste64Dir);
    var root = fileHierarchy.Root;

    foreach (var glbFile in root.FilesWithExtensionRecursive(".glb")) {
      organizer.Add(new GltfModelFileBundle(glbFile).Annotate(glbFile));
    }

    var textureDirectory = root.AssertGetExistingSubdir("Textures");
    foreach (var mapFile in root.AssertGetExistingSubdir("Maps")
                                .GetExistingFiles()) {
      organizer.Add(new Celeste64MapModelFileBundle {
          MapFile = mapFile,
          TextureDirectory = textureDirectory,
      }.Annotate(mapFile));
    }
  }
}