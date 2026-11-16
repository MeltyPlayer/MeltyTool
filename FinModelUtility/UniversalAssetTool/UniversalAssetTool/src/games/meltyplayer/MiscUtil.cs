using fin.io;
using fin.io.bundles;
using fin.model;
using fin.model.io.importers.assimp;
using fin.model.io.importers.gltf;

using gm.api;


namespace uni.games.meltyplayer;

public static class MiscUtil {
  public static void GatherFileBundlesFromHierarchy(
      IFileBundleOrganizer organizer,
      IFileHierarchyDirectory rootDir) {
    foreach (var d3dFile in
             rootDir.FilesWithExtensionsRecursive(".d3d", ".g3d")) {
      var textureFile = new FinFile($"{d3dFile.FullNameWithoutExtension}.png");
      if (!textureFile.Exists) {
        textureFile = null;
      }

      organizer.Add(
          new D3dModelFileBundle {
                  D3dFile = d3dFile,
                  TextureFile = textureFile,
                  TextureWrapMode = WrapMode.REPEAT,
              });
    }

    foreach (var glbFile in rootDir.FilesWithExtension(".glb")) {
      organizer.Add(new GltfModelFileBundle(glbFile) {
          AdditionalProcessing = model => {
            foreach (var texture in model.MaterialManager.Textures) {
              texture.WrapModeU = texture.WrapModeV = WrapMode.REPEAT;
            }
          }
      });
    }

    foreach (var assimpFile in rootDir.FilesWithExtensions(".obj", ".3ds")) {
      organizer.Add(new AssimpModelFileBundle {
          MainFile = assimpFile
      });
    }
  }
}