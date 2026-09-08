using System.Numerics;

using bar.schema;

using fin.data.lazy;
using fin.io;
using fin.model;
using fin.scene;
using fin.util.sets;

using schema.binary;

namespace bar.api;

public sealed record UvctSceneFileBundle(
    IReadOnlyTreeFile MainFile,
    IReadOnlyTreeDirectory RootDirectory)
    : ISceneFileBundle;

public sealed class UvctSceneFileImporter
    : ISceneImporter<UvctSceneFileBundle> {
  public IScene Import(UvctSceneFileBundle fileBundle) {
    var files = fileBundle.MainFile.AsFileSet();
    var finScene = new SceneImpl {
        FileBundle = fileBundle,
        Files = files,
    };
    var finArea = finScene.AddArea();

    var rootNode = finArea.AddRootNode();
    rootNode.SetMatrix(BarUtils.ROOT_MATRIX);

    var lazyUvmdModelDictionary
        = BarUtils.CreateLazyUvmdModelDictionary(
            files,
            fileBundle.RootDirectory);

    AddToScene(fileBundle, files, lazyUvmdModelDictionary, rootNode);

    finScene.CreateDefaultLighting();

    return finScene;
  }

  public static ISceneNode? AddToScene(
      UvctSceneFileBundle fileBundle,
      HashSet<IReadOnlyGenericFile> files,
      ILazyDictionary<short, IReadOnlyModel> lazyUvmdModelDictionary,
      ISceneNode rootNode) {
    var fileChunks
        = fileBundle.MainFile.ReadNew<FileChunks>(Endianness.BigEndian);
    if (fileChunks.Chunks.Count == 0) {
      return null;
    }

    var uvct = new SchemaBinaryReader(fileChunks.Chunks[0].Buffer,
                                      Endianness.BigEndian)
        .ReadNew<Uvct>();

    var rootDirectory = fileBundle.RootDirectory;

    var uvctNode = rootNode.AddChildNode();
    uvctNode.Name = $"UVCT #{fileBundle.MainFile.NameWithoutExtension}";
    uvctNode.SetTag(SceneNodeTag.AREA);

    var finLevelModel = UvmdModelFileImporter.FromMaterialMeshes(
        fileBundle,
        rootDirectory,
        false,
        uvct.MaterialMeshes.Select(m => m.Impl),
        false);
    files.Add(finLevelModel.Files);

    var levelNode = uvctNode.AddChildNode();
    levelNode.AddSceneModel(finLevelModel);
    levelNode.SetTag(SceneNodeTag.LEVEL_GEOMETRY);

    foreach (var uvctModel in uvct.Models) {
      var finModel = lazyUvmdModelDictionary[uvctModel.ModelIndex];
      var uvmdNode = uvctNode.AddChildNode();
      uvmdNode.SetTag(SceneNodeTag.SCENERY_MISC);
      uvmdNode.SetMatrix(
          Matrix4x4.CreateFromQuaternion(
              Quaternion.CreateFromYawPitchRoll(0, MathF.PI / 2, 0)) *
          uvctModel.RdpMatrices[0].ToMatrix4x4());
      uvmdNode.AddSceneModel(finModel);
      uvmdNode.Name = $"UVMD #{uvctModel.ModelIndex}";
    }

    return uvctNode;
  }
}