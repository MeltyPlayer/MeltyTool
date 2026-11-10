using System.Numerics;

using bar.schema;

using fin.color;
using fin.data.lazy;
using fin.io;
using fin.model;
using fin.scene;
using fin.util.sets;

using schema.binary;

namespace bar.api;

public sealed record BarSceneFileBundle(
    IReadOnlyTreeFile MainFile,
    IReadOnlyTreeDirectory RootDirectory,
    int SceneIndex,
    string HumanReadableName)
    : ISceneFileBundle;

public sealed class BarSceneFileImporter
    : ISceneImporter<BarSceneFileBundle> {
  public IScene Import(BarSceneFileBundle fileBundle) {
    var files = fileBundle.MainFile.AsFileSet();
    var finScene = new SceneImpl {
        FileBundle = fileBundle,
        Files = files,
    };
    var finArea = finScene.AddArea();

    var rootNode = finArea.AddRootNode();
    rootNode.SetMatrix(BarUtils.ROOT_MATRIX);

    var fileChunks
        = fileBundle.MainFile.ReadNew<FileChunks>(Endianness.BigEndian);
    var barSceneUvmo = new SchemaBinaryReader(fileChunks.Chunks[1].Buffer,
                                              Endianness.BigEndian)
        .ReadNew<BarSceneUvmo>();
    var barSceneEntry = barSceneUvmo.Entries[fileBundle.SceneIndex];

    var rootDirectory = fileBundle.RootDirectory;
    var lazyUvmdModelDictionary
        = BarUtils.CreateLazyUvmdModelDictionary(files, rootDirectory);
    AddUvtrToScene(
        rootDirectory.AssertGetExistingFile(
            $"uvtr/{barSceneEntry.UvtrIndex}.uvtr"),
        rootDirectory,
        files,
        lazyUvmdModelDictionary,
        rootNode);
    AddUvenToScene(
        rootDirectory.AssertGetExistingFile(
            $"uven/{barSceneEntry.UvenIndex}.uven"),
        rootDirectory,
        files,
        finArea,
        rootNode);

    return finScene;
  }

  public static void AddUvtrToScene(
      IReadOnlyTreeFile uvtrFile,
      IReadOnlyTreeDirectory rootDirectory,
      HashSet<IReadOnlyStandaloneFile> files,
      ILazyDictionary<short, IReadOnlyModel> lazyUvmdModelDictionary,
      ISceneNode rootNode) {
    var fileChunks = uvtrFile.ReadNew<FileChunks>(Endianness.BigEndian);
    var uvtr = new SchemaBinaryReader(fileChunks.Chunks[0].Buffer,
                                      Endianness.BigEndian)
        .ReadNew<Uvtr>();

    var uvtrNode = rootNode.AddChildNode();
    uvtrNode.Name = $"UVTR #{uvtrFile.NameWithoutExtension}";

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
          uvtrNode);
      node?.SetMatrix(uvtrCell.Data.Transform);
    }
  }

  public static void AddUvenToScene(
      IReadOnlyTreeFile uvenFile,
      IReadOnlyTreeDirectory rootDirectory,
      HashSet<IReadOnlyStandaloneFile> files,
      ISceneArea area,
      ISceneNode rootNode) {
    var fileChunks = uvenFile.ReadNew<FileChunks>(Endianness.BigEndian);
    var uven = new SchemaBinaryReader(fileChunks.Chunks[0].Buffer,
                                      Endianness.BigEndian)
        .ReadNew<Uven>();

    var uvenNode = rootNode.AddChildNode();
    uvenNode.Name = $"UVEN #{uvenFile.NameWithoutExtension}";

    area.CreateCustomSkyboxNode();
    area.BackgroundColor = uven.ClearColor.ToSystemColor();

    foreach (var uvmdTuple in uven.Uvmds) {
      var finModel = UvmdModelFileImporter.Import(
          new UvmdModelFileBundle(
              rootDirectory.AssertGetExistingFile(
                  $"uvmd/{uvmdTuple.UvmdIndex}.uvmd"),
              rootDirectory),
          false);
      files.Add(finModel.Files);
      var uvmdNode = uvenNode.AddChildNode();
      uvmdNode.AddSceneModel(finModel);
      uvmdNode.SetMatrix(
          Matrix4x4.CreateFromQuaternion(
              Quaternion.CreateFromYawPitchRoll(0, MathF.PI / 2, 0)));
      uvmdNode.Name = $"UVMD #{uvmdTuple.UvmdIndex}";
    }
  }
}