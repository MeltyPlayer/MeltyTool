using System.Drawing;

using fin.data.lazy;
using fin.image;
using fin.language.equations.fixedFunction;
using fin.language.equations.fixedFunction.impl;
using fin.model;
using fin.util.asserts;

namespace gx {
  /// <summary>
  ///   BMD material, one of the common formats for the GameCube.
  ///
  ///   For more info:
  ///   http://www.amnoid.de/gc/tev.html
  /// </summary>
  public partial class GxFixedFunctionMaterial {
    private const bool STRICT = false;

    public override string ToString() => this.Material.Name ?? "(n/a)";

    public GxFixedFunctionMaterial(
        IModel model,
        IMaterialManager materialManager,
        IPopulatedMaterial populatedMaterial,
        IList<IGxTexture> tex1Textures,
        ILazyDictionary<(IGxTexture, ITexCoordGen?, ITextureMatrixInfo?),
            ITexture> lazyTextureDictionary) {
      // TODO: materialEntry.Flag determines draw order

      var materialName = populatedMaterial.Name;

      var textures =
          populatedMaterial.TextureIndices?
                           .Select(i => i != -1 ? tex1Textures[i] : null)
                           .ToArray()
          ?? tex1Textures;

      var material = materialManager.AddFixedFunctionMaterial();
      material.Name = materialName;
      material.CullingMode =
          populatedMaterial.CullMode switch {
              GxCullMode.None  => CullingMode.SHOW_BOTH,
              GxCullMode.Front => CullingMode.SHOW_BACK_ONLY,
              GxCullMode.Back  => CullingMode.SHOW_FRONT_ONLY,
              GxCullMode.All   => CullingMode.SHOW_NEITHER,
              _                => throw new ArgumentOutOfRangeException(),
          };

      var depthFunction = populatedMaterial.DepthFunction;
      material.DepthMode =
          depthFunction.Enable
              ? (depthFunction.WriteNewValueIntoDepthBuffer
                  ? DepthMode.USE_DEPTH_BUFFER
                  : DepthMode.SKIP_WRITE_TO_DEPTH_BUFFER)
              : DepthMode.IGNORE_DEPTH_BUFFER;
      material.DepthCompareType = depthFunction.Func.ToFinDepthCompareType();

      new GxFixedFunctionBlending().ApplyBlending(
          material,
          populatedMaterial.BlendMode.BlendMode,
          populatedMaterial.BlendMode.SrcFactor,
          populatedMaterial.BlendMode.DstFactor,
          populatedMaterial.BlendMode.LogicOp);
      material.SetAlphaCompare(
          populatedMaterial.AlphaCompare.MergeFunc.ToFinAlphaOp(),
          populatedMaterial.AlphaCompare.Func0.ToFinAlphaCompareType(),
          populatedMaterial.AlphaCompare.Reference0,
          populatedMaterial.AlphaCompare.Func1.ToFinAlphaCompareType(),
          populatedMaterial.AlphaCompare.Reference1);

      this.Material = material;

      var colorConstants = new List<Color>();

      var equations = material.Equations;
      var registers = Asserts.CastNonnull(materialManager.Registers);

      var colorZero = equations.CreateColorConstant(0);

      var scZero = equations.CreateScalarConstant(0);
      var scOne = equations.CreateScalarConstant(1);
      var scTwo = equations.CreateScalarConstant(2);
      var scFour = equations.CreateScalarConstant(4);
      var scHalf = equations.CreateScalarConstant(.5);
      var scMinusHalf = equations.CreateScalarConstant(-.5);
      var sc255 = equations.CreateScalarConstant(255);
      var sc255Sqr = equations.CreateScalarConstant(256 * 255);

      var colorFixedFunctionOps = new ColorFixedFunctionOps(equations);
      var scalarFixedFunctionOps = new ScalarFixedFunctionOps(equations);

      var valueManager = new ValueManager(equations, registers);

      valueManager.SetColorRegisters(populatedMaterial.ColorRegisters);
      valueManager.SetKonstColors(populatedMaterial.KonstColors);

      var vertexColors = new IColorValue[2];
      var vertexAlphas = new IScalarValue[2];
      for (byte i = 0; i < 2; i++) {
        vertexColors[i] = equations.CreateOrGetColorInput(
            FixedFunctionSource.VERTEX_COLOR_0 + i);
        vertexAlphas[i] = equations.CreateOrGetScalarInput(
            FixedFunctionSource.VERTEX_ALPHA_0 + i);
      }

      for (var i = 0; i < 4; ++i) {
        var colorChannelControl = populatedMaterial.ColorChannelControls?[i];
        if (colorChannelControl == null) {
          continue;
        }

        var activeLights =
            colorChannelControl.LitMask.GetActiveLights().ToArray();

        // TODO: Properly handle lights and attenuation and stuff

        // TODO: Expose material/ambient registers to side panel

        if (i % 2 == 0) {
          var colorIndex = (byte) (i / 2);

          var vertexColor = vertexColors[colorIndex];

          var materialColor = populatedMaterial.MaterialColors[colorIndex];
          var materialColorRegisterValue =
              colorChannelControl.MaterialSrc switch {
                  GxColorSrc.Register => registers.GetOrCreateColorRegister(
                      $"GxMaterialColor{colorIndex}",
                      equations.CreateColorConstant(
                          materialColor.R / 255.0,
                          materialColor.G / 255.0,
                          materialColor.B / 255.0)
                  ),
                  GxColorSrc.Vertex => vertexColor,
              };

          var colorValue = materialColorRegisterValue;

          var isLightingEnabled = colorChannelControl.LightingEnabled;
          if (isLightingEnabled) {
            var ambientColor = populatedMaterial.AmbientColors[colorIndex];
            var ambientColorRegisterValue =
                colorChannelControl.AmbientSrc switch {
                    GxColorSrc.Register => registers.GetOrCreateColorRegister(
                        $"GxAmbientColor{colorIndex}",
                        equations.CreateColorConstant(
                            ambientColor.R / 255.0,
                            ambientColor.G / 255.0,
                            ambientColor.B / 255.0)),
                    GxColorSrc.Vertex => vertexColor,
                };

            // TODO: Factor in how colors are merged in channel control
            IColorValue? mergedLightColor = null;
            // TODO: Should these be averaged?
            foreach (var activeLight in activeLights) {
              var lightSrc = FixedFunctionSource.LIGHT_DIFFUSE_COLOR_0 +
                             activeLight;
              mergedLightColor = colorFixedFunctionOps.Add(
                  mergedLightColor,
                  equations.CreateOrGetColorInput(lightSrc));
            }

            var illuminationColor =
                colorFixedFunctionOps.Add(mergedLightColor,
                                          ambientColorRegisterValue);
            if (illuminationColor != null) {
              illuminationColor.Clamp = true;
            }

            colorValue =
                colorFixedFunctionOps.Multiply(materialColorRegisterValue,
                                               illuminationColor);
          }

          var color = colorValue ?? colorZero;
          valueManager.UpdateColorChannelColor(
              colorIndex switch {
                  0 => GxColorChannel.GX_COLOR0A0,
                  1 => GxColorChannel.GX_COLOR1A1,
              },
              color);
          valueManager.UpdateColorChannelColor(
              colorIndex switch {
                  0 => GxColorChannel.GX_COLOR0,
                  1 => GxColorChannel.GX_COLOR1,
              },
              color);
        } else {
          var alphaIndex = (byte) ((i - 1) / 2);

          var vertexAlpha = vertexAlphas[alphaIndex];

          var materialColor = populatedMaterial.MaterialColors[alphaIndex];
          var materialAlphaRegisterValue =
              colorChannelControl.MaterialSrc switch {
                  GxColorSrc.Register => registers.GetOrCreateScalarRegister(
                      $"GxMaterialAlpha{alphaIndex}",
                      equations.CreateScalarConstant(
                          materialColor.A / 255.0)),
                  GxColorSrc.Vertex => vertexAlpha,
              };

          var alphaValue = materialAlphaRegisterValue;

          var isLightingEnabled = colorChannelControl.LightingEnabled;
          if (isLightingEnabled) {
            var ambientColor = populatedMaterial.AmbientColors[alphaIndex];
            var ambientAlphaRegisterValue =
                colorChannelControl.AmbientSrc switch {
                    GxColorSrc.Register => registers.GetOrCreateScalarRegister(
                        $"GxAmbientAlpha{alphaIndex}",
                        equations.CreateScalarConstant(
                            ambientColor.A / 255.0)),
                    GxColorSrc.Vertex => vertexAlpha,
                };

            // TODO: Factor in how colors are merged in channel control
            IScalarValue? mergedLightAlpha = null;
            // TODO: Should these be averaged?
            foreach (var activeLight in activeLights) {
              var lightSrc = FixedFunctionSource.LIGHT_DIFFUSE_ALPHA_0 +
                             activeLight;
              mergedLightAlpha = scalarFixedFunctionOps.Add(
                  mergedLightAlpha,
                  equations.CreateOrGetScalarInput(lightSrc));
            }

            var illuminationAlpha =
                scalarFixedFunctionOps.Add(mergedLightAlpha,
                                           ambientAlphaRegisterValue);
            if (illuminationAlpha != null) {
              illuminationAlpha.Clamp = true;
            }

            alphaValue =
                scalarFixedFunctionOps.Multiply(
                    materialAlphaRegisterValue,
                    illuminationAlpha);
          }

          var alpha = alphaValue ?? scZero;
          valueManager.UpdateColorChannelAlpha(
              alphaIndex switch {
                  0 => GxColorChannel.GX_COLOR0A0,
                  1 => GxColorChannel.GX_COLOR1A1,
              },
              alpha
          );
          valueManager.UpdateColorChannelAlpha(
              alphaIndex switch {
                  0 => GxColorChannel.GX_ALPHA0,
                  1 => GxColorChannel.GX_ALPHA1,
              },
              alpha
          );
        }
      }

      for (var i = 0; i < populatedMaterial.TevStageInfos.Length; ++i) {
        var tevStage = populatedMaterial.TevStageInfos[i];
        if (tevStage == null) {
          continue;
        }

        //var tevSwapMode = populatedMaterial.TevSwapModes[i];
        var tevOrder = populatedMaterial.TevOrderInfos[i];

        // Updates which texture is referred to by TEXC
        var textureIndex = tevOrder.TexMap;
        if (textureIndex == -1 || (!STRICT && textureIndex >= textures.Count)) {
          valueManager.UpdateTextureIndex(null);
        } else {
          var bmdTexture = textures[textureIndex];

          var texCoordGen =
              populatedMaterial.TexCoordGens[tevOrder.TexCoordId]!;

          var texMatrixType = texCoordGen.TexMatrix;
          var texMatrixIndex = (texMatrixType - GxTexMatrix.TexMtx0) / 3;
          var texMatrix = texMatrixType != GxTexMatrix.Identity
              ? populatedMaterial.TextureMatrices[texMatrixIndex]
              : null;

          var texture =
              lazyTextureDictionary[(bmdTexture, texCoordGen, texMatrix)];

          valueManager.UpdateTextureIndex(textureIndex);
          material.SetTextureSource(textureIndex, texture);
        }

        // Updates which color is referred to by RASC
        var colorChannel = tevOrder.ColorChannelId;
        valueManager.UpdateRascChannel(colorChannel);

        // Updates which values are referred to by konst
        valueManager.UpdateKonst(tevOrder.KonstColorSel,
                                 tevOrder.KonstAlphaSel);

        // Set up color logic
        {
          var colorA = valueManager.GetColor(tevStage.color_a);
          var colorB = valueManager.GetColor(tevStage.color_b);
          var colorC = valueManager.GetColor(tevStage.color_c);
          var colorD = valueManager.GetColor(tevStage.color_d);

          IColorValue? colorValue = null;

          var colorOp = tevStage.color_op;
          switch (colorOp) {
            // ADD: out = a*(1 - c) + b*c + d
            case TevOp.GX_TEV_ADD:
            case TevOp.GX_TEV_SUB: {
              var bias = tevStage.color_bias switch {
                  TevBias.GX_TB_ZERO    => null,
                  TevBias.GX_TB_ADDHALF => scHalf,
                  TevBias.GX_TB_SUBHALF => scMinusHalf,
                  _ => throw new ArgumentOutOfRangeException(
                      "Unsupported color bias!")
              };

              var scale = tevStage.color_scale switch {
                  TevScale.GX_CS_SCALE_1  => scOne,
                  TevScale.GX_CS_SCALE_2  => scTwo,
                  TevScale.GX_CS_SCALE_4  => scFour,
                  TevScale.GX_CS_DIVIDE_2 => scHalf,
                  _ => throw new ArgumentOutOfRangeException(
                      "Unsupported color scale!")
              };

              colorValue =
                  colorFixedFunctionOps.AddOrSubtractOp(
                      colorOp == TevOp.GX_TEV_ADD,
                      colorA,
                      colorB,
                      colorC,
                      colorD,
                      bias,
                      scale
                  );

              colorValue ??= colorZero;
              colorValue.Clamp = tevStage.color_clamp;

              break;
            }

            case TevOp.GX_TEV_COMP_R8_GT: {
              colorValue = colorFixedFunctionOps.Add(
                  colorD,
                  colorA.R.TernaryOperator(
                      BoolComparisonType.GREATER_THAN,
                      colorB.R,
                      colorC,
                      colorZero));
              break;
            }
            case TevOp.GX_TEV_COMP_R8_EQ: {
              colorValue = colorFixedFunctionOps.Add(
                  colorD,
                  colorA.R.TernaryOperator(
                      BoolComparisonType.EQUAL_TO,
                      colorB.R,
                      colorC,
                      colorZero));
              break;
            }

            case TevOp.GX_TEV_COMP_GR16_GT: {
              var valueA = scalarFixedFunctionOps.Add(
                  scalarFixedFunctionOps.Multiply(colorA.G, sc255Sqr),
                  scalarFixedFunctionOps.Multiply(colorA.R, sc255)) ?? scZero;
              var valueB = scalarFixedFunctionOps.Add(
                  scalarFixedFunctionOps.Multiply(colorB.G, sc255Sqr),
                  scalarFixedFunctionOps.Multiply(colorB.R, sc255)) ?? scZero;

              colorValue = colorFixedFunctionOps.Add(
                  colorD,
                  valueA.TernaryOperator(
                      BoolComparisonType.GREATER_THAN,
                      valueB,
                      colorC,
                      colorZero));
              break;
            }

            default: {
              if (GxFixedFunctionMaterial.STRICT) {
                throw new NotImplementedException();
              } else {
                colorValue = colorC;
              }

              break;
            }
          }

          valueManager.UpdateColorRegister(tevStage.color_regid,
                                           colorValue ?? colorZero);
        }

        // Set up alpha logic
        {
          var alphaA = valueManager.GetAlpha(tevStage.alpha_a);
          var alphaB = valueManager.GetAlpha(tevStage.alpha_b);
          var alphaC = valueManager.GetAlpha(tevStage.alpha_c);
          var alphaD = valueManager.GetAlpha(tevStage.alpha_d);

          IScalarValue? alphaValue = null;

          // TODO: Switch this to an enum
          var alphaOp = tevStage.alpha_op;
          switch (alphaOp) {
            // ADD: out = a*(1 - c) + b*c + d
            case TevOp.GX_TEV_ADD:
            case TevOp.GX_TEV_SUB: {
              var bias = tevStage.alpha_bias switch {
                  TevBias.GX_TB_ZERO    => null,
                  TevBias.GX_TB_ADDHALF => scHalf,
                  TevBias.GX_TB_SUBHALF => scMinusHalf,
                  _ => throw new ArgumentOutOfRangeException(
                      "Unsupported alpha bias!")
              };

              var scale = tevStage.alpha_scale switch {
                  TevScale.GX_CS_SCALE_1  => scOne,
                  TevScale.GX_CS_SCALE_2  => scTwo,
                  TevScale.GX_CS_SCALE_4  => scFour,
                  TevScale.GX_CS_DIVIDE_2 => scHalf,
                  _ => throw new ArgumentOutOfRangeException(
                      "Unsupported alpha scale!")
              };

              alphaValue =
                  scalarFixedFunctionOps.AddOrSubtractOp(
                      alphaOp == TevOp.GX_TEV_ADD,
                      alphaA,
                      alphaB,
                      alphaC,
                      alphaD,
                      bias,
                      scale
                  );

              alphaValue ??= scZero;
              alphaValue.Clamp = tevStage.alpha_clamp;

              break;
            }

            default: {
              if (GxFixedFunctionMaterial.STRICT) {
                throw new NotImplementedException();
              } else {
                alphaValue = scZero;
              }

              break;
            }
          }

          valueManager.UpdateAlphaRegister(tevStage.alpha_regid, alphaValue);
        }
      }

      equations.CreateColorOutput(
          FixedFunctionSource.OUTPUT_COLOR,
          valueManager.GetColor(GxCc.GX_CC_CPREV));

      equations.CreateScalarOutput(
          FixedFunctionSource.OUTPUT_ALPHA,
          valueManager.GetAlpha(GxCa.GX_CA_APREV));

      // TODO: Set up compiled texture?
      // TODO: If only a const color, create a texture for that

      var colorTextureCount =
          material.Textures.Count(
              texture => texture.ColorType == ColorType.COLOR);

      // TODO: This is a bad assumption!
      if (colorTextureCount == 0 && colorConstants.Count > 0) {
        var colorConstant = colorConstants.Last();

        var intensityTexture = material.Textures
                                       .FirstOrDefault(
                                           texture => texture.ColorType ==
                                               ColorType.INTENSITY);
        if (intensityTexture != null) {
          return;
        }

        var colorImage = FinImage.Create1x1FromColor(colorConstant);
        var colorTexture = materialManager.CreateTexture(colorImage);
        material.CompiledTexture = colorTexture;
      }
    }

    public IMaterial Material { get; }
  }
}