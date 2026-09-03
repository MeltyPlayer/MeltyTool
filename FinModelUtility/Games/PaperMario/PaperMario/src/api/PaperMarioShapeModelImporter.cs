using System.Numerics;

using f3dzex2.combiner;
using f3dzex2.displaylist;
using f3dzex2.displaylist.opcodes;
using f3dzex2.displaylist.opcodes.f3dzex2;
using f3dzex2.image;
using f3dzex2.io;
using f3dzex2.model;

using fin.data.dictionaries;
using fin.data.lazy;
using fin.data.queues;
using fin.image.util;
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
        Rsp = new Rsp {
            GeometryMode = 0,
        }
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

    var dlReader = new DisplayListReader();
    var dlModelBuilder = new DlModelBuilder(n64Hardware, fileBundle, files);
    var finModel = dlModelBuilder.Model;

    TextureArchive? textureArchive = null;
    if (assetsDirectory.TryToGetExistingFile(
            $"{shapeFile.Name.SubstringUpTo('_')}_tex",
            out var textureFile) ||
        assetsDirectory.TryToGetExistingFile(
            $"{shapeFile.Name.SubstringUpTo('_')}__tex",
            out textureFile)) {
      textureArchive
          = textureFile.ReadNew<TextureArchive>(Endianness.BigEndian);
      files.Add(textureFile);
    }

    var texEnvDictionary
        = textureArchive?.TextureEnvironments.ToDictionary(t => t.Name);

    var lazyTextures = new LazyDictionary<Image, IReadOnlyTexture>(image => {
      var finTexture = finModel.MaterialManager.CreateTexture(image.Mipmaps);

      finTexture.WrapModeU = image.WrapModeS.AsFinWrapMode(1);
      finTexture.WrapModeV = image.WrapModeT.AsFinWrapMode(1);

      return finTexture;
    });

    var modelTreeNodeQueue = new FinTuple2Queue<IBone, ModelTreeNode>(
        (finModel.Skeleton.Root, shape.ModelTreeRoot));
    while (modelTreeNodeQueue.TryDequeue(out var parentFinBone,
                                         out var modelTreeNode)) {
      if (modelTreeNode.Type is InternalType.LEAF) {
        var propertiesById
            = modelTreeNode.Properties.ToListDictionary(p => p.Id);

        var renderModeProperty
            = propertiesById.GetSingleOrDefault(PropertyId.RENDER_MODE);
        var renderMode
            = renderModeProperty?.Value.AssertAsA<IntProperty>().Value ?? 0;

        var transparencyType = renderMode switch {
            1 or 4                => TransparencyType.OPAQUE,
            5 or 7 or 0xD or 0x10 => TransparencyType.MASK,
            _                     => TransparencyType.TRANSPARENT
        };
        var usesAlpha = transparencyType != TransparencyType.OPAQUE;

        var texEnvNameProperty
            = propertiesById.GetSingleOrDefault(PropertyId.TEX_ENV_NAME);
        var texEnvName = texEnvNameProperty?.Value.AssertAsA<StringProperty>()
                                           .Value;
        if (texEnvName != null) {
          var texEnv = texEnvDictionary.AssertNonnull()[texEnvName];

          var (image0, image1) = texEnv.Images;

          rdp.Tmem.HardcodedTexture0 = lazyTextures[image0];
          rdp.Tmem.HardcodedTexture1
              = image1 != null ? lazyTextures[image1] : null;

          switch (texEnv.CombineMode) {
            // Modulate
            case 0 or 8: {
              rdp.SetCombinerCycleParams(
                  (new() {
                       ColorMuxA = GenericColorMux.G_CCMUX_TEXEL0,
                       ColorMuxB = GenericColorMux.G_CCMUX_0,
                       ColorMuxC = GenericColorMux.G_CCMUX_TEXEL1,
                       ColorMuxD = GenericColorMux.G_CCMUX_0,
                       AlphaMuxA = usesAlpha
                           ? GenericAlphaMux.G_ACMUX_TEXEL0
                           : GenericAlphaMux.G_ACMUX_1,
                       AlphaMuxB = GenericAlphaMux.G_ACMUX_0,
                       AlphaMuxC = usesAlpha
                           ? GenericAlphaMux.G_ACMUX_TEXEL1
                           : GenericAlphaMux.G_ACMUX_1,
                       AlphaMuxD = GenericAlphaMux.G_ACMUX_0,
                   },
                   new() {
                       ColorMuxA = GenericColorMux.G_CCMUX_COMBINED,
                       ColorMuxB = GenericColorMux.G_CCMUX_0,
                       ColorMuxC = GenericColorMux.G_CCMUX_SHADE,
                       ColorMuxD = GenericColorMux.G_CCMUX_0,
                       AlphaMuxA = GenericAlphaMux.G_ACMUX_COMBINED,
                       AlphaMuxB = GenericAlphaMux.G_ACMUX_0,
                       AlphaMuxC = GenericAlphaMux.G_ACMUX_SHADE,
                       AlphaMuxD = GenericAlphaMux.G_ACMUX_0,
                   }));
              break;
            }
            // Difference
            case 0xD: {
              rdp.SetCombinerCycleParams(
                  (new() {
                      ColorMuxA = GenericColorMux.G_CCMUX_0,
                      ColorMuxB = GenericColorMux.G_CCMUX_0,
                      ColorMuxC = GenericColorMux.G_CCMUX_0,
                      ColorMuxD = GenericColorMux.G_CCMUX_SHADE,
                      AlphaMuxA = GenericAlphaMux.G_ACMUX_TEXEL0,
                      AlphaMuxB = GenericAlphaMux.G_ACMUX_TEXEL1,
                      AlphaMuxC = GenericAlphaMux.G_ACMUX_SHADE,
                      AlphaMuxD = GenericAlphaMux.G_ACMUX_0,
                  }, null));
              break;
            }
            // Interp
            case 0x10: {
              rdp.SetCombinerCycleParams(
                  (new() {
                      ColorMuxA = GenericColorMux.G_CCMUX_1,
                      ColorMuxB = GenericColorMux.G_CCMUX_SHADE_ALPHA,
                      ColorMuxC = GenericColorMux.G_CCMUX_TEXEL0,
                      ColorMuxD = GenericColorMux.G_CCMUX_0,
                      AlphaMuxA = GenericAlphaMux.G_ACMUX_0,
                      AlphaMuxB = GenericAlphaMux.G_ACMUX_0,
                      AlphaMuxC = GenericAlphaMux.G_ACMUX_0,
                      AlphaMuxD = GenericAlphaMux.G_ACMUX_TEXEL0,
                  }, new() {
                      ColorMuxA = GenericColorMux.G_CCMUX_SHADE_ALPHA,
                      ColorMuxB = GenericColorMux.G_CCMUX_0,
                      ColorMuxC = GenericColorMux.G_CCMUX_TEXEL0,
                      ColorMuxD = GenericColorMux.G_CCMUX_COMBINED,
                      AlphaMuxA = GenericAlphaMux.G_ACMUX_0,
                      AlphaMuxB = GenericAlphaMux.G_ACMUX_0,
                      AlphaMuxC = GenericAlphaMux.G_ACMUX_0,
                      AlphaMuxD = GenericAlphaMux.G_ACMUX_COMBINED,
                  }));
              break;
            }
            default: {
              rdp.SetSimpleCombinerCycleParams(true, true, usesAlpha);
              break;
            }
          }
        } else {
          rdp.Tmem.HardcodedTexture0 = null;
          rdp.Tmem.HardcodedTexture1 = null;

          rdp.SetSimpleCombinerCycleParams(false, true, usesAlpha);
        }

        rdp.ZMode = transparencyType switch {
            TransparencyType.OPAQUE      => ZMode.ZMODE_OPA,
            TransparencyType.MASK        => ZMode.ZMODE_DEC,
            TransparencyType.TRANSPARENT => ZMode.ZMODE_XLU,
            _                            => throw new ArgumentOutOfRangeException()
        };
        if (transparencyType == TransparencyType.TRANSPARENT) {
          rdp.P0 = rdp.P1 = BlenderPm.G_BL_CLR_IN;
          rdp.A0 = rdp.A1 = BlenderA.G_BL_A_IN;
          rdp.M0 = rdp.M1 = BlenderPm.G_BL_CLR_MEM;
          rdp.B0 = rdp.B1 = BlenderB.G_BL_1MA;
        } else {
          rdp.P0 = rdp.P1 = BlenderPm.G_BL_CLR_MEM;
          rdp.A0 = rdp.A1 = BlenderA.G_BL_0;
          rdp.M0 = rdp.M1 = BlenderPm.G_BL_CLR_IN;
          rdp.B0 = rdp.B1 = BlenderB.G_BL_1;
        }

        n64Hardware.Rsp.ActiveBoneWeights
            = finModel.Skin.GetOrCreateBoneWeights(
                VertexSpace.RELATIVE_TO_BONE,
                parentFinBone);

        var displayList = dlReader.ReadDisplayList(
            n64Hardware.Memory,
            new F3dzex2OpcodeParser(),
            modelTreeNode.DisplayListOffset);
        dlModelBuilder.AddDl(displayList);
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