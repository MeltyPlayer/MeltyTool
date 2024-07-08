using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

using fin.color;
using fin.language.equations.fixedFunction;
using fin.util.image;

namespace fin.model.impl;

public partial class ModelImpl<TVertex> {
  private partial class MaterialManagerImpl {
    public IFixedFunctionMaterial AddFixedFunctionMaterial() {
      this.Registers ??= new FixedFunctionRegisters();
      var material = new FixedFunctionMaterialImpl(this.Registers);
      this.materials_.Add(material);
      return material;
    }
  }

  private class FixedFunctionMaterialImpl
      : BMaterialImpl, IFixedFunctionMaterial {
    private readonly List<ITexture> textures_ = [];

    private readonly ITexture?[] texturesSources_ = new ITexture[8];
    private readonly IColor?[] colors_ = new IColor[2];
    private readonly float?[] alphas_ = new float?[2];

    public FixedFunctionMaterialImpl(IFixedFunctionRegisters registers) {
      this.Textures = new ReadOnlyCollection<ITexture>(this.textures_);
      this.Registers = registers;

      this.TextureSources =
          new ReadOnlyCollection<ITexture?>(this.texturesSources_);
      this.TransparencyType = TransparencyType.TRANSPARENT;
    }

    public override IEnumerable<ITexture> Textures { get; }

    public IFixedFunctionEquations<FixedFunctionSource> Equations { get; } =
      new FixedFunctionEquations<FixedFunctionSource>();

    public IFixedFunctionRegisters Registers { get; }

    public IReadOnlyList<ITexture?> TextureSources { get; }

    public IFixedFunctionMaterial SetTextureSource(
        int textureIndex,
        ITexture texture) {
      if (!this.texturesSources_.Contains(texture)) {
        this.textures_.Add(texture);
      }

      this.texturesSources_[textureIndex] = texture;

      return this;
    }

    public ITexture? CompiledTexture { get; set; }

    public IFixedFunctionMaterial SetBlending(
        BlendEquation blendEquation,
        BlendFactor srcFactor,
        BlendFactor dstFactor,
        LogicOp logicOp)
      => this.SetBlendingSeparate(blendEquation,
                                  srcFactor,
                                  dstFactor,
                                  blendEquation,
                                  srcFactor,
                                  dstFactor,
                                  logicOp);

    public IFixedFunctionMaterial SetBlendingSeparate(
        BlendEquation colorBlendEquation,
        BlendFactor colorSrcFactor,
        BlendFactor colorDstFactor,
        BlendEquation alphaBlendEquation,
        BlendFactor alphaSrcFactor,
        BlendFactor alphaDstFactor,
        LogicOp logicOp) {
      this.ColorBlendEquation = colorBlendEquation;
      this.ColorSrcFactor = colorSrcFactor;
      this.ColorDstFactor = colorDstFactor;
      this.AlphaBlendEquation = alphaBlendEquation;
      this.AlphaSrcFactor = alphaSrcFactor;
      this.AlphaDstFactor = alphaDstFactor;
      this.LogicOp = logicOp;
      return this;
    }

    public BlendEquation ColorBlendEquation { get; private set; } =
      BlendEquation.ADD;

    public BlendFactor ColorSrcFactor { get; private set; } =
      BlendFactor.SRC_ALPHA;

    public BlendFactor ColorDstFactor { get; private set; } =
      BlendFactor.ONE_MINUS_SRC_ALPHA;

    public BlendEquation AlphaBlendEquation { get; private set; } =
      BlendEquation.ADD;

    public BlendFactor AlphaSrcFactor { get; private set; } =
      BlendFactor.ONE;

    public BlendFactor AlphaDstFactor { get; private set; } =
      BlendFactor.ONE;

    public LogicOp LogicOp { get; private set; } = LogicOp.UNDEFINED;


    public IFixedFunctionMaterial SetAlphaCompare(
        AlphaOp alphaOp,
        AlphaCompareType alphaCompareType0,
        float reference0,
        AlphaCompareType alphaCompareType1,
        float reference1) {
      this.AlphaOp = alphaOp;
      this.AlphaCompareType0 = alphaCompareType0;
      this.AlphaReference0 = reference0;
      this.AlphaCompareType1 = alphaCompareType1;
      this.AlphaReference1 = reference1;
      return this;
    }

    public AlphaOp AlphaOp { get; private set; }

    public AlphaCompareType AlphaCompareType0 { get; private set; } =
      AlphaCompareType.Always;

    public float AlphaReference0 { get; private set; }

    public AlphaCompareType AlphaCompareType1 { get; private set; } =
      AlphaCompareType.Always;

    public float AlphaReference1 { get; private set; }
  }
}