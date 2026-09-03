using System.Drawing;

using f3dzex2.image;
using f3dzex2.io;

using fin.io;
using fin.scene;

using pm.schema.fileTable.maps;
using pm.schema.maps;

using schema.binary;

namespace pm.api;

public sealed record PaperMarioMapSceneFileBundle(
    IReadOnlyTreeFile AreaFile,
    IReadOnlyTreeFile MapFile,
    IReadOnlyTreeFile RomOverlayFile,
    IReadOnlyTreeDirectory AssetsDirectory)
    : ISceneFileBundle {
  public IReadOnlyTreeFile MainFile => this.MapFile;

  public IEnumerable<IReadOnlyGenericFile> Files => [
      this.AreaFile,
      this.MapFile,
      this.RomOverlayFile,
  ];
}

/// <summary>
///   Shamelessly stolen from:
///   https://github.com/magcius/noclip.website/blob/main/src/PaperMario64/tools/extractor.ts
/// </summary>
public sealed class PaperMarioMapSceneImporter
    : ISceneImporter<PaperMarioMapSceneFileBundle> {
  public IScene Import(PaperMarioMapSceneFileBundle fileBundle) {
    var files = fileBundle.Files.ToHashSet();
    var finScene = new SceneImpl {
        FileBundle = fileBundle,
        Files = files,
    };

    var finArea = finScene.AddArea();

    var pmArea = fileBundle.AreaFile.Deserialize<Area>();
    var pmMap = fileBundle.MapFile.Deserialize<Map>();

    var assetsDirectory = fileBundle.AssetsDirectory;

    if ((pmMap.BackgroundName ?? "") != "") {
      var bgFile = assetsDirectory.AssertGetExistingFile(pmMap.BackgroundName);
      files.Add(bgFile);

      var bgBr = bgFile.OpenReadAsBinary(Endianness.BigEndian);
      var bg = bgBr.ReadNew<Background>();

      bgBr.Position = bg.ImageOffset;
      var textureData = bgBr.ReadBytes(bgBr.Length - bgBr.Position);

      bgBr.Position = bg.PaletteOffset;
      var paletteData = bgBr.ReadBytes(bgBr.Length - bgBr.Position);

      var n64Hardware = new N64Hardware<SeparateN64Memory> {
          Memory = new(),
          Rdp = new Rdp {
              PaletteSegmentedAddress = 0,
          }
      };

      n64Hardware.Memory.AddSegment(0, 0, paletteData);

      var image = new N64ImageParser(n64Hardware)
          .Parse(N64ImageFormat.CI8, textureData, bg.Width, bg.Height);

      finArea.BackgroundImage = image;
    } else {
      finArea.BackgroundColor = Color.Black;
      finArea.CreateCustomSkyboxNode();
    }

    var mapName = pmMap.MapName;
    var mapPrefix = mapName == "dgb_00" ? "arn_20" : mapName;

    if (assetsDirectory.TryToGetExistingFile(
            $"{mapPrefix}_shape",
            out var shapeFile)) {
      finArea.AddRootNode()
             .AddSceneModel(PaperMarioShapeModelImporter.Import(
                                fileBundle,
                                shapeFile,
                                assetsDirectory,
                                files));
    }

    return finScene;
  }
}