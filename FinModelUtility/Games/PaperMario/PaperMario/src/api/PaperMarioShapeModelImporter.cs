using System.Numerics;

using f3dzex2.displaylist;
using f3dzex2.displaylist.opcodes.f3dzex2;
using f3dzex2.image;
using f3dzex2.io;
using f3dzex2.model;

using fin.data.queues;
using fin.io;
using fin.io.bundles;
using fin.model;
using fin.model.io;
using fin.model.io.importers;
using fin.model.util;
using fin.util.asserts;
using fin.util.strings;

using pm.schema.maps;

using schema.binary;

namespace pm.api;

public sealed record PaperMarioShapeModelFileBundle(
    IReadOnlyTreeFile ShapeFile,
    IReadOnlyTreeDirectory AssetsDirectory)
    : IModelFileBundle {
  public IReadOnlyTreeFile MainFile => this.ShapeFile;
}

/// <summary>
///   Shamelessly stolen from:
///   https://github.com/magcius/noclip.website/blob/main/src/PaperMario64/tools/extractor.ts
/// </summary>
public sealed class PaperMarioShapeModelImporter
    : IModelImporter<PaperMarioShapeModelFileBundle> {
  public IModel Import(PaperMarioShapeModelFileBundle fileBundle)
    => Import(fileBundle, fileBundle.ShapeFile, fileBundle.AssetsDirectory, []);

  public static IModel Import(
      IFileBundle fileBundle,
      IReadOnlyTreeFile shapeFile,
      IReadOnlyTreeDirectory assetsDirectory,
      HashSet<IReadOnlyGenericFile> files) {
    files.Add(shapeFile);

    using var shapeBr = shapeFile.OpenReadAsBinary(Endianness.BigEndian);
    var shape = shapeBr.ReadNew<Shape>();
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

    var rdp = n64Hardware.Rdp = new Rdp {
        Tmem = new NoclipTmem(n64Hardware),
    };
    rdp.SetSimpleCombinerCycleParams(false, false, false);

    var dlReader = new DisplayListReader();
    var dlModelBuilder = new DlModelBuilder(n64Hardware, fileBundle, files);
    var finModel = dlModelBuilder.Model;

    TextureArchive? textureArchive = null;
    if (assetsDirectory.TryToGetExistingFile(
            $"{shapeFile.Name.SubstringUpTo('_')}_tex",
            out var textureFile)) {
      textureArchive = textureFile.ReadNew<TextureArchive>(Endianness.BigEndian);
      files.Add(textureFile);
    }

    var texEnvDictionary
        = textureArchive?.TextureEnvironments.ToDictionary(t => t.Name);

    var modelTreeNodeQueue = new FinTuple2Queue<IBone, ModelTreeNode>(
        (finModel.Skeleton.Root, shape.ModelTreeRoot));
    while (modelTreeNodeQueue.TryDequeue(out var parentFinBone,
                                         out var modelTreeNode)) {
      if (modelTreeNode.Type is InternalType.LEAF) {
        var texEnvNameProperty
            = modelTreeNode.Properties.SingleOrDefault(p => p.Id is PropertyId
                  .TEX_ENV_NAME);
        var texEnvName = texEnvNameProperty?.Value.AssertAsA<StringProperty>()
                                           .Value;

        /*if (texEnvName != null) {
          var texEnv = texEnvDictionary.AssertNonnull()[texEnvName];

          rdp.Tmem.HardcodedTexture0 = texEnv.
        }*/

        var displayList = dlReader.ReadDisplayList(
            n64Hardware.Memory,
            new F3dzex2OpcodeParser(),
            modelTreeNode.DisplayListOffset);
        dlModelBuilder.AddDl(displayList);

        n64Hardware.Rsp.ActiveBoneWeights
            = finModel.Skin.GetOrCreateBoneWeights(
                VertexSpace.RELATIVE_TO_BONE,
                parentFinBone);
      } else {
        var groupData = modelTreeNode.GroupData.AssertNonnull();
        var currentFinBone = parentFinBone.AddChild(
            groupData.ModelMatrix?.ToMatrix4x4() ??
            Matrix4x4.Identity);

        modelTreeNodeQueue.Enqueue(
            groupData.Children.Select(childModelTreeNode
                                          => (currentFinBone,
                                              childModelTreeNode)));
      }
    }

    return finModel;
  }
}