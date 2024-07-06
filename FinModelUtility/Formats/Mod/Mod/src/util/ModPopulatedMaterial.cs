using System;
using System.Drawing;
using System.Linq;

using fin.color;
using fin.math;
using fin.schema.vector;
using fin.util.enums;

using gx;

using mod.api;
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
      var hasChannel0 = tevInfo.TevStages.Any(
          s => s.ColorChannel is GxColorChannel.GX_COLOR0
                                 or GxColorChannel.GX_COLOR0A0
                                 or GxColorChannel.GX_ALPHA0);
      var hasChannel1 = tevInfo.TevStages.Any(
          s => s.ColorChannel is GxColorChannel.GX_COLOR1
                                 or GxColorChannel.GX_COLOR1A1
                                 or GxColorChannel.GX_ALPHA1);
      var hasBothLightingAndVertexColor = hasChannel0 && hasChannel1;

      var attenuationFunction = lightingSpecularEnabled
          ? GxAttenuationFunction.Spec
          : GxAttenuationFunction.Spot;
      var litMask = GxLightMask.Light0 |
                    GxLightMask.Light1 |
                    GxLightMask.Light2;

      var useVertexColor
          = lightingFlags.CheckFlag(LightingInfoFlags.USE_VERTEX_COLOR);
      var useVertexAlpha
          = lightingFlags.CheckFlag(LightingInfoFlags.USE_VERTEX_ALPHA);

      var lightColorSrc
          = useVertexColor ? GxColorSrc.Vertex : GxColorSrc.Register;
      var lightAlphaSrc
          = useVertexAlpha ? GxColorSrc.Vertex : GxColorSrc.Register;

      this.ColorChannelControls = [
          new ColorChannelControlImpl {
              LightingEnabled = lightingEnabled,
              MaterialSrc = lightColorSrc,
              AmbientSrc = lightColorSrc,
              LitMask = litMask,
              AttenuationFunction = attenuationFunction,
          },
          new ColorChannelControlImpl {
              LightingEnabled = lightingEnabled,
              MaterialSrc = lightAlphaSrc,
              AmbientSrc = lightAlphaSrc,
              LitMask = litMask,
              AttenuationFunction = attenuationFunction,
          },
          new ColorChannelControlImpl {
              // Seems to sometimes be vertex color????
              LightingEnabled
                  = lightingEnabled && !hasBothLightingAndVertexColor,
              MaterialSrc = lightColorSrc,
              AmbientSrc = lightColorSrc,
              LitMask = litMask,
              AttenuationFunction = attenuationFunction,
              VertexColorIndex = 0,
          },
          new ColorChannelControlImpl {
              // Seems to sometimes be vertex color????
              LightingEnabled
                  = lightingEnabled && !hasBothLightingAndVertexColor,
              MaterialSrc = lightAlphaSrc,
              AmbientSrc = lightAlphaSrc,
              LitMask = litMask,
              AttenuationFunction = attenuationFunction,
              VertexColorIndex = 0,
          }
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
                    10 => GxTexMatrix.Identity,
                    >= 0 and < 8
                        => (GxTexMatrix) ((int) GxTexMatrix.TexMtx0 +
                                          3 * tex.TexMatrix),
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
        this.TextureWrapModeOverrides
            = material.texInfo.TexturesInMaterial
                      .Select(tex => (tex.WrapModeS, tex.WrapModeT))
                      .ToArray();
      }

      {
        var peInfo = material.peInfo;

        if (material.flags.CheckFlag(MaterialFlags.TRANSPARENT_BLEND)) {
          this.BlendMode = new BlendFunctionImpl {
              BlendMode = GxBlendMode.BLEND,
              SrcFactor = GxBlendFactor.SRC_ALPHA,
              DstFactor = GxBlendFactor.ONE_MINUS_SRC_ALPHA,
              LogicOp = GxLogicOp.SET,
          };
        } else {
          this.BlendMode = new BlendFunctionImpl {
              BlendMode = peInfo.BlendMode,
              SrcFactor = peInfo.SrcFactor,
              DstFactor = peInfo.DstFactor,
              LogicOp = peInfo.LogicOp,
          };
        }

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

    public short[] TextureIndices { get; }
    public ITexCoordGen[] TexCoordGens { get; }
    public ITextureMatrixInfo?[] TextureMatrices { get; }

    public (GxWrapMode wrapModeS, GxWrapMode wrapModeT)[]?
        TextureWrapModeOverrides { get; }

    public IDepthFunction DepthFunction { get; }
  }
}