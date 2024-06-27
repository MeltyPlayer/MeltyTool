using System.Drawing;
using System.Linq;

using fin.color;

using gx;

using mod.schema.mod;

namespace mod.util;

internal class ModPopulatedMaterial : IPopulatedMaterial {
  public ModPopulatedMaterial(Material material, TEVInfo tevInfo) {
      // TODO: Where does this come from?
      this.CullMode = GxCullMode.Back;

      this.KonstColors =
        tevInfo.KonstColors.Select(FinColor.ToSystemColor).ToArray();
      this.ColorRegisters =
        tevInfo.ColorRegisters
        .Select(reg => reg.unknown1)
        .Select((rgba, i) => (IColorRegister) new GxColorRegister {
            Color = Color.FromArgb(rgba.A, rgba.R, rgba.G, rgba.B),
            Index = i,
        })
        .ToArray();

      // TODO: This is a guess
      var materialColor = FinColor.ToSystemColor(material.colourInfo.diffuseColour);
      this.MaterialColors = [materialColor, materialColor];

      // TODO: This is a guess

      this.TevStageInfos = tevInfo.TevStages.Select(tevStage => {
        var colorCombiner = tevStage.ColorCombiner;
        var alphaCombiner = tevStage.AlphaCombiner;

        return new TevStagePropsImpl {
          color_a = colorCombiner.colorA,
          color_b = colorCombiner.colorB,
          color_c = colorCombiner.colorC,
          color_d = colorCombiner.colorD,
          color_op = colorCombiner.colorOp,
          color_regid = colorCombiner.colorRegister,

          alpha_a = alphaCombiner.alphaA,
          alpha_b = alphaCombiner.alphaB,
          alpha_c = alphaCombiner.alphaC,
          alpha_d = alphaCombiner.alphaD,
          alpha_op = alphaCombiner.alphaOp,
          alpha_regid = alphaCombiner.alphaRegister
        };
      }).ToArray();

      this.TevOrderInfos = tevInfo.TevStages.Select(tevStage => {
        return new TevOrderImpl {
          TexMap = tevStage.TexMap,
          TexCoordId = tevStage.TexCoordId,
          ColorChannelId = tevStage.ColorChannel,
        };
      }).ToArray();

      var colorChannelControl = new ColorChannelControlImpl {
        LightingEnabled = false,
        MaterialSrc = GxColorSrc.Vertex,
      };
      this.ColorChannelControls = [
          colorChannelControl,
        colorChannelControl,
        colorChannelControl,
        colorChannelControl
      ];

      this.TexCoordGens = material.texInfo.unknown3.Select(tex =>
      new TexCoordGenImpl {
        TexGenSrc = tex.TexGenSrc
      }).ToArray();

      this.TextureIndices = material.texInfo.TexturesInMaterial.Select(tex => (short)tex.TexAttrIndex).ToArray();
    }


  public string Name => "material";
  public GxCullMode CullMode { get; }
  public Color[] MaterialColors { get; }
  public IColorChannelControl?[] ColorChannelControls { get; }
  public Color[] AmbientColors { get; }
  public Color?[] LightColors { get; } = [];
  public Color[] KonstColors { get; }
  public IColorRegister[] ColorRegisters { get; }
  public ITevOrder?[] TevOrderInfos { get; }
  public ITevStageProps?[] TevStageInfos { get; }
  public ITevSwapMode?[] TevSwapModes { get; }
  public ITevSwapModeTable?[] TevSwapModeTables { get; }
  public IAlphaCompare AlphaCompare { get; } = new AlphaCompareImpl {
      MergeFunc = GxAlphaOp.Or,
      Func0 = GxCompareType.Greater,
      Reference0 = .5f,
      Func1 = GxCompareType.Never,
      Reference1 = 0f,
  };
  public IBlendFunction BlendMode { get; } = new BlendFunctionImpl {
      BlendMode = GxBlendMode.NONE,
      DstFactor = GxBlendFactor.ONE_MINUS_SRC_ALPHA,
      SrcFactor = GxBlendFactor.SRC_ALPHA,
      LogicOp = GxLogicOp.COPY,
  };
  public ITexCoordGen?[] TexCoordGens { get; }
  public ITextureMatrixInfo?[] TextureMatrices { get; }
  public IDepthFunction DepthFunction { get; } = new DepthFunctionImpl {
      Enable = true,
      Func = GxCompareType.Less,
      WriteNewValueIntoDepthBuffer = true,
  };
  public short[] TextureIndices { get; }
}