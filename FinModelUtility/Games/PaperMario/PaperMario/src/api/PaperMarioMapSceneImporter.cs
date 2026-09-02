using f3dzex2.displaylist;
using f3dzex2.displaylist.opcodes.f3dzex2;
using f3dzex2.image;
using f3dzex2.io;
using f3dzex2.model;

using fin.data.queues;
using fin.io;
using fin.scene;
using fin.ui.rendering.gl.scene;
using fin.util.asserts;

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

      var n64Hardware = new N64Hardware<SeparateN64Memory>();
      n64Hardware.Memory = new();
      n64Hardware.Rdp = new Rdp {
          PaletteSegmentedAddress = 0,
      };

      n64Hardware.Memory.AddSegment(0, 0, paletteData);

      var image = new N64ImageParser(n64Hardware)
          .Parse(N64ImageFormat.CI8, textureData, bg.Width, bg.Height);

      finArea.BackgroundImage = image;
    }

    var mapName = pmMap.MapName;
    var mapPrefix = mapName == "dgb_00" ? "arn_20" : mapName;
    if (assetsDirectory.TryToGetExistingFile(
            $"{mapPrefix}_shape",
            out var shapeFile)) {
      using var shapeBr = shapeFile.OpenReadAsBinary(Endianness.BigEndian);

      var shape = shapeBr.ReadNew<Shape>();

      var dlReader = new DisplayListReader();

      var shapeFileBytes = shapeFile.ReadAllBytes();

      var n64Hardware = new N64Hardware<SeparateN64Memory> {
          Rsp = new Rsp(),
      };

      var n64Memory = n64Hardware.Memory = new SeparateN64Memory();
      n64Memory.AddSegment(0, 0, shapeFileBytes);

      IoUtils.SplitSegmentedAddress(Shape.BASE_RAM_ADDRESS,
                                    out var ramSegment,
                                    out var ramOffset);
      n64Memory.AddSegment(ramSegment, ramOffset, shapeFileBytes);

      n64Hardware.Rdp = new Rdp {
          Tmem = new NoclipTmem(n64Hardware),
      };

      var modelTreeNodeQueue = new FinTuple2Queue<ISceneNode?, ModelTreeNode>(
          (null, shape.ModelTreeRoot));
      while (modelTreeNodeQueue.TryDequeue(out var parentFinNode,
                                           out var modelTreeNode)) {
        if (modelTreeNode.Type is InternalType.LEAF) {
          var texEnvNameProperty = modelTreeNode.Properties.SingleOrDefault(p => p.Id is PropertyId.TEX_ENV_NAME);
          var texEnvName = texEnvNameProperty?.Value.AssertAsA<StringProperty>().Value;



          var displayList = dlReader.ReadDisplayList(
              n64Hardware.Memory,
              new F3dzex2OpcodeParser(),
              modelTreeNode.DisplayListOffset);

          var dlModelBuilder
              = new DlModelBuilder(n64Hardware, fileBundle, files);
          dlModelBuilder.AddDl(displayList);

          parentFinNode
              .AssertNonnull()
              .AddComponent(
                  new SimpleModelRenderComponent(dlModelBuilder.Model));
        } else {
          var currentNode = parentFinNode != null
              ? parentFinNode.AddChildNode()
              : finArea.AddRootNode();

          var groupData = modelTreeNode.GroupData.AssertNonnull();

          if (groupData is { ModelMatrix: { } modelMatrix }) {
            currentNode.SetMatrix(modelMatrix.ToMatrix4x4());
          }

          modelTreeNodeQueue.Enqueue(
              groupData.Children.Select(childModelTreeNode
                                            => (currentNode,
                                                childModelTreeNode)));
        }
      }
    }

    return finScene;
  }
}