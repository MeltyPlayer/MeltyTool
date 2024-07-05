using System;
using System.Drawing;
using System.Linq;

using fin.color;
using fin.math;
using fin.schema.vector;
using fin.util.enums;

using gx;

using mod.schema.mod;

namespace mod.util {
  internal class ModPopulatedMaterial : IPopulatedMaterial {
    public ModPopulatedMaterial(int materialIndex,
                                Material material,
                                TEVInfo tevInfo) {
      // TODO: Where does this come from?

      this.CullMode = GxCullMode.Back;

      this.KonstColors =
          tevInfo.KonstColors.Select(FinColor.ToSystemColor).ToArray();
      this.ColorRegisters =
          tevInfo.ColorRegisters
                 .Select(reg => reg.Color)
                 .Select((rgba, i) => (IColorRegister) new GxColorRegister {
                     Color = Color.FromArgb(
                         rgba.A.Clamp((ushort) 0, (ushort) 255),
                         rgba.R.Clamp((ushort) 0, (ushort) 255),
                         rgba.G.Clamp((ushort) 0, (ushort) 255),
                         rgba.B.Clamp((ushort) 0, (ushort) 255)),
                     Index = i,
                 })
                 .ToArray();

      // TODO: This is a guess
      {
        // Not sure where this comes from, used for various things:
        //   - Pikmin color
        //   - Armored cannon beetle eye color
        var unkColor = (12345, Color.FromArgb(255, 0, 0, 0));

        var materialColor = (materialIndex,
                             FinColor.ToSystemColor(
                                 material.ColorInfo.DiffuseColour));
        var ambientColor
            = (materialIndex,
               FinColor.ToSystemColor(FinColor.FromIntensityFloat(.1f)));
        this.MaterialColors = [materialColor, unkColor];
        this.AmbientColors = [ambientColor, unkColor];
      }

      this.TevStageInfos = tevInfo.TevStages.Select(tevStage => {
                                    var colorCombiner = tevStage.ColorCombiner;
                                    var alphaCombiner = tevStage.AlphaCombiner;

                                    return new TevStagePropsImpl {
                                        color_a = colorCombiner.colorA,
                                        color_b = colorCombiner.colorB,
                                        color_c = colorCombiner.colorC,
                                        color_d = colorCombiner.colorD,
                                        color_op = colorCombiner.colorOp,
                                        color_bias = colorCombiner.colorBias,
                                        color_scale = colorCombiner.colorScale,
                                        color_clamp = colorCombiner.colorClamp,
                                        color_regid
                                            = colorCombiner.colorRegister,

                                        alpha_a = alphaCombiner.alphaA,
                                        alpha_b = alphaCombiner.alphaB,
                                        alpha_c = alphaCombiner.alphaC,
                                        alpha_d = alphaCombiner.alphaD,
                                        alpha_op = alphaCombiner.alphaOp,
                                        alpha_bias = alphaCombiner.alphaBias,
                                        alpha_scale = alphaCombiner.alphaScale,
                                        alpha_clamp = alphaCombiner.alphaClamp,
                                        alpha_regid
                                            = alphaCombiner.alphaRegister
                                    };
                                  })
                                  .ToArray();

      this.TevOrderInfos = tevInfo.TevStages.Select(tevStage => {
                                    var texMap = tevStage.TexMap;
                                    var texCoordId
                                        = GxTexCoord.GX_TEXCOORD_NULL;
                                    var texGenData
                                        = material.texInfo.TexGenData;
                                    if ((sbyte) texMap >= 0 &&
                                        (sbyte) texMap < texGenData.Length) {
                                      texCoordId = material.texInfo
                                          .TexGenData[(int) texMap].TexCoordId;
                                    }

                                    if (tevStage.TexCoord !=
                                        GxTexCoord.GX_TEXCOORD_NULL) {
                                      texCoordId = tevStage.TexCoord;
                                    }

                                    var colorChannel = tevStage.ColorChannel;
                                    if ((byte) colorChannel == 255) {
                                      colorChannel
                                          = GxColorChannel.GX_COLOR0A0;
                                    }

                                    var tevOrderImpl = new TevOrderImpl {
                                        TexMap = texMap,
                                        TexCoordId = texCoordId,
                                        ColorChannelId = colorChannel,
                                        KonstColorSel
                                            = tevStage.KonstColorSelection,
                                        KonstAlphaSel
                                            = tevStage.KonstAlphaSelection,
                                    };

                                    return tevOrderImpl;
                                  })
                                  .ToArray();

      var lightingInfo = material.lightingInfo;
      var lightingFlags = lightingInfo.typeFlags;
      var lightingEnabled = lightingFlags.CheckFlag(LightingInfoFlags.ENABLED);
      var lightingSpecularEnabled
          = lightingFlags.CheckFlag(LightingInfoFlags.SPECULAR_ENABLED);
      var srcFor0 = GxColorSrc.Register;
      var srcFor1 = GxColorSrc.Vertex;

      // TODO: Stupid hack, how to do this better???
      var hasLightingColor = tevInfo.TevStages.Any(
          s => s.ColorChannel is GxColorChannel.GX_COLOR0
                                 or GxColorChannel.GX_COLOR0A0
                                 or GxColorChannel.GX_ALPHA0);
      var hasVertexColor = tevInfo.TevStages.Any(
          s => s.ColorChannel is GxColorChannel.GX_COLOR1
                                 or GxColorChannel.GX_COLOR1A1
                                 or GxColorChannel.GX_ALPHA1);
      var hasBothLightingAndVertexColor = hasLightingColor && hasVertexColor;

      var attenuationFunction = lightingSpecularEnabled
          ? GxAttenuationFunction.Spec
          : GxAttenuationFunction.Spot;
      var litMask = GxLightMask.Light0 |
                    GxLightMask.Light1 |
                    GxLightMask.Light2;
      var colorChannelControl0 = new ColorChannelControlImpl {
          LightingEnabled = lightingEnabled,
          MaterialSrc = GxColorSrc.Register,
          AmbientSrc = GxColorSrc.Register,
          LitMask = litMask,
          AttenuationFunction = attenuationFunction,
      };
      var colorChannelControl1 = new ColorChannelControlImpl {
          // Seems to sometimes be vertex color????
          LightingEnabled = lightingEnabled && !hasBothLightingAndVertexColor,
          MaterialSrc = GxColorSrc.Register,
          AmbientSrc = GxColorSrc.Register,
          LitMask = litMask,
          AttenuationFunction = attenuationFunction,
          VertexColorIndex = 0,
      };
      this.ColorChannelControls = [
          colorChannelControl0,
          colorChannelControl0,
          colorChannelControl1,
          colorChannelControl1
      ];

      {
        this.TextureIndices = material.texInfo.TexturesInMaterial
                                      .Select(tex => (short) tex.TexAttrIndex)
                                      .ToArray();
        this.TexCoordGens
            = material
              .texInfo
              .TexGenData
              .Select(tex => {
                var texMatrix = tex.TexMatrix switch {
                    10           => GxTexMatrix.Identity,
                    >= 0 and < 8 => GxTexMatrix.TexMtx0 + tex.TexMatrix,
                };
                return new TexCoordGenImpl(
                    GxTexGenType.Matrix2x4,
                    tex.TexGenSrc,
                    texMatrix);
              })
              .ToArray();
        this.TextureMatrices
            = material.texInfo.TexturesInMaterial.Select(
                          t => new TextureMatrixInfoImpl(
                              GxTexGenType.Matrix2x4,
                              new Vector2f {
                                  X = 1 / t.Scale.X,
                                  Y = 1 / t.Scale.Y
                              },
                              new Vector2f {
                                  X = -t.Position.X,
                                  Y = -t.Position.Y
                              },
                              (short) (t.Rotation / MathF.PI * 32768f)
                          ))
                      .ToArray();
      }

      {
        var peInfo = material.peInfo;

        this.BlendMode = new BlendFunctionImpl {
            BlendMode = peInfo.BlendMode,
            DstFactor = peInfo.DstFactor,
            SrcFactor = peInfo.SrcFactor,
            LogicOp = peInfo.LogicOp,
        };
        this.AlphaCompare = new AlphaCompareImpl {
            MergeFunc = peInfo.AlphaCompareOp,
            Func0 = peInfo.CompareType0,
            Reference0 = peInfo.Reference0,
            Func1 = peInfo.CompareType1,
            Reference1 = peInfo.Reference1,
        };
        this.DepthFunction = new DepthFunctionImpl {
            Enable = peInfo.Enable,
            Func = peInfo.DepthCompareType,
            WriteNewValueIntoDepthBuffer = peInfo.WriteNewIntoBuffer,
        };
      }
    }


    public string Name => "material";
    public GxCullMode CullMode { get; }
    public (int, Color)[] MaterialColors { get; }
    public IColorChannelControl?[] ColorChannelControls { get; }
    public (int, Color)[] AmbientColors { get; }
    public Color?[] LightColors { get; } = [];
    public Color[] KonstColors { get; }
    public IColorRegister[] ColorRegisters { get; }
    public ITevOrder?[] TevOrderInfos { get; }
    public ITevStageProps?[] TevStageInfos { get; }
    public ITevSwapMode?[] TevSwapModes { get; }
    public ITevSwapModeTable?[] TevSwapModeTables { get; }

    public IAlphaCompare AlphaCompare { get; }
    public IBlendFunction BlendMode { get; }

    public ITexCoordGen[] TexCoordGens { get; }
    public ITextureMatrixInfo?[] TextureMatrices { get; }

    public IDepthFunction DepthFunction { get; }

    public short[] TextureIndices { get; }
  }
}