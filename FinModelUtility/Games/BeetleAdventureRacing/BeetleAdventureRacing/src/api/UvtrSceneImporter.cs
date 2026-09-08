using System.Numerics;

using bar.schema;

using fin.io;
using fin.scene;
using fin.util.sets;

using schema.binary;

namespace bar.api;

public sealed record UvtrSceneFileBundle(
    IReadOnlyTreeFile MainFile,
    IReadOnlyTreeDirectory RootDirectory,
    string? HumanReadableName)
    : ISceneFileBundle;

public sealed class UvtrSceneFileImporter
    : ISceneImporter<UvtrSceneFileBundle> {
  public IScene Import(UvtrSceneFileBundle fileBundle) {
    var files = fileBundle.MainFile.AsFileSet();
    var finScene = new SceneImpl {
        FileBundle = fileBundle,
        Files = files,
    };
    var finArea = finScene.AddArea();

    var rootNode = finArea.AddRootNode();
    rootNode.SetMatrix(
        Matrix4x4.CreateFromQuaternion(
            Quaternion.CreateFromYawPitchRoll(0, -MathF.PI / 2, 0)));

    var fileChunks
        = fileBundle.MainFile.ReadNew<FileChunks>(Endianness.BigEndian);
    var uvtr = new SchemaBinaryReader(fileChunks.Chunks[0].Buffer,
                                      Endianness.BigEndian)
        .ReadNew<Uvtr>();

    var lazyUvmdModelDictionary
        = BarUtils.CreateLazyUvmdModelDictionary(
            files,
            fileBundle.RootDirectory);

    var rootDirectory = fileBundle.RootDirectory;

    foreach (var uvtrCell in uvtr.Cells) {
      if (!uvtrCell.HasData) {
        continue;
      }

      var node = UvctSceneFileImporter.AddToScene(
          new UvctSceneFileBundle(
              rootDirectory.AssertGetExistingFile(
                  $"uvct/{uvtrCell.Data.UvctIndex}.uvct"),
              rootDirectory),
          files,
          lazyUvmdModelDictionary,
          rootNode);
      node?.SetMatrix(uvtrCell.Data.Transform);
    }

    finScene.CreateDefaultLighting();

    return finScene;
  }
}