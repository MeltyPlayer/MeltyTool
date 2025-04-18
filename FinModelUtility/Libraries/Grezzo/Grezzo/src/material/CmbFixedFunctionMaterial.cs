using System;
using System.Linq;

using fin.data.lazy;
using fin.image;
using fin.image.formats;
using fin.model;
using fin.image.util;

using grezzo.schema.cmb;

using SixLabors.ImageSharp.PixelFormats;

using BlendEquation = grezzo.schema.cmb.BlendEquation;
using BlendFactor = grezzo.schema.cmb.BlendFactor;
using FinBlendEquation = fin.model.BlendEquation;
using FinBlendFactor = fin.model.BlendFactor;
using FinTextureMinFilter = fin.model.TextureMinFilter;
using FinTextureMagFilter = fin.model.TextureMagFilter;
using TextureMagFilter = grezzo.schema.cmb.TextureMagFilter;
using TextureMinFilter = grezzo.schema.cmb.TextureMinFilter;

namespace grezzo.material;

public class CmbFixedFunctionMaterial {
  private const bool USE_FIXED_FUNCTION = true;
  private const bool USE_JANKY_TRANSPARENCY = false;

  public CmbFixedFunctionMaterial(
      IModel finModel,
      Cmb cmb,
      int materialIndex,
      bool hasVertexColors,
      ILazyArray<IImage> textureImages) {
      var mats = cmb.mats.Data;
      var cmbMaterials = mats.Materials;
      var cmbMaterial = cmbMaterials[materialIndex];

      // Get associated texture
      var finTextures =
          cmbMaterial
              .texMappers.Select((texMapper, i) => {
                var textureId = texMapper.textureId;

                ITexture? finTexture = null;
                if (textureId != -1) {
                  var cmbTexture = cmb.tex.Data.textures[textureId];

                  var rawTextureImage = textureImages[textureId];

                  // TODO: Is this logic possibly right????
                  IImage textureImage;
                  if (TransparencyTypeUtil.GetTransparencyType(rawTextureImage) !=
                      TransparencyType.OPAQUE || !USE_JANKY_TRANSPARENCY) {
                    textureImage = rawTextureImage;
                  } else {
                    var backgroundColor = texMapper.BorderColor;

                    var processedImage = new Rgba32Image(
                        rawTextureImage.PixelFormat,
                        rawTextureImage.Width,
                        rawTextureImage.Height);
                    textureImage = processedImage;

                    rawTextureImage.Access(srcGetHandler => {
                      using var dstLock = processedImage.Lock();
                      var dstPtr = dstLock.Pixels;
                      for (var y = 0; y < rawTextureImage.Height; y++) {
                        for (var x = 0; x < rawTextureImage.Width; x++) {
                          srcGetHandler(x,
                                        y,
                                        out var r,
                                        out var g,
                                        out var b,
                                        out var a);

                          if (r == backgroundColor.Rb &&
                              g == backgroundColor.Gb &&
                              b == backgroundColor.Bb) {
                            a = 0;
                          }

                          dstPtr[y * rawTextureImage.Width + x] =
                              new Rgba32(r, g, b, a);
                        }
                      }
                    });
                  }

                  var cmbTexCoord = cmbMaterial.texCoords[i];

                  finTexture =
                      finModel.MaterialManager.CreateTexture(textureImage);
                  finTexture.Name = cmbTexture.name;
                  finTexture.WrapModeU = this.CmbToFinWrapMode(texMapper.wrapS);
                  finTexture.WrapModeV = this.CmbToFinWrapMode(texMapper.wrapT);
                  finTexture.MinFilter =
                      texMapper.minFilter switch {
                          TextureMinFilter.Nearest =>
                              FinTextureMinFilter.NEAR,
                          TextureMinFilter.Linear =>
                              FinTextureMinFilter.LINEAR,
                          TextureMinFilter
                                  .NearestMipmapNearest =>
                              FinTextureMinFilter
                                  .NEAR_MIPMAP_NEAR,
                          TextureMinFilter
                                  .LinearMipmapNearest =>
                              FinTextureMinFilter
                                  .LINEAR_MIPMAP_NEAR,
                          TextureMinFilter
                                  .NearestMipmapLinear =>
                              FinTextureMinFilter
                                  .NEAR_MIPMAP_LINEAR,
                          TextureMinFilter
                                  .LinearMipmapLinear =>
                              FinTextureMinFilter
                                  .LINEAR_MIPMAP_LINEAR,
                          _ => throw new ArgumentOutOfRangeException()
                      };
                  finTexture.MagFilter =
                      texMapper.magFilter switch {
                          TextureMagFilter.Nearest =>
                              FinTextureMagFilter.NEAR,
                          TextureMagFilter.Linear =>
                              FinTextureMagFilter.LINEAR,
                          _ => throw new ArgumentOutOfRangeException()
                      };
                  finTexture.LodBias = texMapper.lodBias;
                  finTexture.MinLod = texMapper.minLodBias;
                  finTexture.UvIndex = cmbTexCoord.coordinateIndex;

                  if (finTexture.UvIndex == 1) {
                    ;
                  }
                  finTexture.BorderColor = texMapper.BorderColor;

                  finTexture.UvType =
                      cmbTexCoord.mappingMethod ==
                      TextureMappingType.UvCoordinateMap
                          ? UvType.STANDARD
                          : UvType.SPHERICAL;

                  // TODO: Better way to specify this??
                  if (cmbTexCoord.scale.Y == 2 &&
                      texMapper.wrapT == TextureWrapMode.Mirror) {
                    cmbTexCoord.translation.Y -= 1;
                  }

                  finTexture.SetTranslation2d(cmbTexCoord.translation.X,
                                        cmbTexCoord.translation.Y);
                  finTexture.SetRotationRadians2d(cmbTexCoord.rotation);
                  finTexture.SetScale2d(cmbTexCoord.scale.X,
                                        cmbTexCoord.scale.Y);

                  // TODO: Use LUTs/Distribution in specular calculation
                }

                return finTexture;
              })
              .ToArray();

      // Create material
      if (!USE_FIXED_FUNCTION) {
        // TODO: Remove this hack
        var firstTexture = finTextures.FirstOrDefault();
        var firstColorFinTexture = finTextures.FirstOrDefault(tex => {
          var image = tex?.Image;
          if (image == null) {
            return false;
          }

          var isAllBlank = true;

          image.Access(getHandler => {
            for (var y = 0; y < image.Height; ++y) {
              for (var x = 0; x < image.Width; ++x) {
                getHandler(x, y, out var r, out var g, out var b, out var a);
                if (!(a == 0 || (r == 255 && g == 255 && b == 255))) {
                  isAllBlank = false;
                  return;
                }
              }
            }
          });

          return !isAllBlank;
        });


        var bestTexture = firstColorFinTexture ?? firstTexture;
        var finMaterial = bestTexture != null
            ? (IMaterial) finModel.MaterialManager.AddTextureMaterial(
                bestTexture)
            : finModel.MaterialManager.AddNullMaterial();
        this.Material = finMaterial;
      } else {
        var finMaterial = finModel.MaterialManager.AddFixedFunctionMaterial();
        this.Material = finMaterial;

        for (var i = 0; i < finTextures.Length; ++i) {
          var finTexture = finTextures[i];
          if (finTexture != null) {
            finMaterial.SetTextureSource(i, finTexture);
          }
        }

        var combinerGenerator =
            new CmbCombinerGenerator(cmbMaterial, hasVertexColors, finMaterial);

        var combiners = mats.Combiners;
        var texEnvStages =
            cmbMaterial.texEnvStagesIndices.Select(
                           i => {
                             if (i == -1) {
                               return null;
                             }

                             if (i < 0 || i >= combiners.Length) {
                               ;
                             }

                             return mats.Combiners[i];
                           })
                       .ToArray();

        combinerGenerator.AddCombiners(texEnvStages);

        if (!cmbMaterial.alphaTestEnabled) {
          finMaterial.SetAlphaCompare(AlphaOp.Or,
                                      AlphaCompareType.Always,
                                      0,
                                      AlphaCompareType.Always,
                                      0);
        } else {
          finMaterial.SetAlphaCompare(
              cmbMaterial.alphaTestFunction switch {
                  TestFunc.Always   => AlphaCompareType.Always,
                  TestFunc.Equal    => AlphaCompareType.Equal,
                  TestFunc.Gequal   => AlphaCompareType.GEqual,
                  TestFunc.Greater  => AlphaCompareType.Greater,
                  TestFunc.Never    => AlphaCompareType.Never,
                  TestFunc.Less     => AlphaCompareType.Less,
                  TestFunc.Lequal   => AlphaCompareType.LEqual,
                  TestFunc.Notequal => AlphaCompareType.NEqual,
                  _                 => throw new ArgumentOutOfRangeException()
              },
              cmbMaterial.alphaTestReferenceValue);
        }

        finMaterial.UpdateAlphaChannel = false;

        // TODO: Fix these
        switch (cmbMaterial.blendMode) {
          case BlendMode.BlendNone: {
            finMaterial.SetBlending(FinBlendEquation.ADD,
                                    FinBlendFactor.ONE,
                                    FinBlendFactor.ZERO,
                                    LogicOp.UNDEFINED);
            break;
          }
          case BlendMode.Blend: {
            finMaterial.SetBlending(
                this.CmbBlendEquationToFin(cmbMaterial.colorEquation),
                this.CmbBlendFactorToFin(cmbMaterial.colorSrcFunc),
                this.CmbBlendFactorToFin(cmbMaterial.colorDstFunc),
                LogicOp.UNDEFINED);
            break;
          }
          case BlendMode.LogicalOp:
          case BlendMode.BlendSeparate: {
            finMaterial.SetBlendingSeparate(
                this.CmbBlendEquationToFin(cmbMaterial.colorEquation),
                this.CmbBlendFactorToFin(cmbMaterial.colorSrcFunc),
                this.CmbBlendFactorToFin(cmbMaterial.colorDstFunc),
                this.CmbBlendEquationToFin(cmbMaterial.alphaEquation),
                this.CmbBlendFactorToFin(cmbMaterial.alphaSrcFunc),
                this.CmbBlendFactorToFin(cmbMaterial.alphaDstFunc),
                LogicOp.UNDEFINED);
            break;
          }
          default:                      throw new ArgumentOutOfRangeException();
        }

        finMaterial.DepthCompareType = cmbMaterial.depthTestFunction switch {
            TestFunc.Never    => DepthCompareType.Never,
            TestFunc.Less     => DepthCompareType.Less,
            TestFunc.Equal    => DepthCompareType.Equal,
            TestFunc.Lequal   => DepthCompareType.LEqual,
            TestFunc.Greater  => DepthCompareType.Greater,
            TestFunc.Notequal => DepthCompareType.NEqual,
            TestFunc.Gequal   => DepthCompareType.GEqual,
            TestFunc.Always   => DepthCompareType.Always,
        };

        finMaterial.DepthMode = DepthMode.NONE;
        if (cmbMaterial.depthTestEnabled) {
          finMaterial.DepthMode |= DepthMode.READ;
        }

        if (cmbMaterial.depthWriteEnabled) {
          finMaterial.DepthMode |= DepthMode.WRITE;
        }
      }

      this.Material.Name = $"material{materialIndex}";
      this.Material.CullingMode = cmbMaterial.faceCulling switch {
          CullMode.FrontAndBack => CullingMode.SHOW_NEITHER,
          CullMode.Front        => CullingMode.SHOW_BACK_ONLY,
          CullMode.Back         => CullingMode.SHOW_FRONT_ONLY,
          CullMode.Never        => CullingMode.SHOW_BOTH,
          _                     => throw new ArgumentOutOfRangeException()
      };
    }

  public IMaterial Material { get; }

  public WrapMode CmbToFinWrapMode(TextureWrapMode cmbMode)
    => cmbMode switch {
        TextureWrapMode.ClampToBorder => WrapMode.CLAMP,
        TextureWrapMode.Repeat        => WrapMode.REPEAT,
        TextureWrapMode.ClampToEdge   => WrapMode.CLAMP,
        TextureWrapMode.Mirror        => WrapMode.MIRROR_REPEAT,
        _                             => throw new ArgumentOutOfRangeException(nameof(cmbMode), cmbMode, null)
    };

  public FinBlendEquation CmbBlendEquationToFin(
      BlendEquation cmbBlendEquation)
    => cmbBlendEquation switch {
        BlendEquation.FuncAdd      => FinBlendEquation.ADD,
        BlendEquation.FuncSubtract => FinBlendEquation.SUBTRACT,
        BlendEquation.FuncReverseSubtract => FinBlendEquation
            .REVERSE_SUBTRACT,
        BlendEquation.Min => FinBlendEquation.MIN,
        BlendEquation.Max => FinBlendEquation.MAX,
        _ => throw new ArgumentOutOfRangeException(
            nameof(cmbBlendEquation),
            cmbBlendEquation,
            null)
    };

  public FinBlendFactor CmbBlendFactorToFin(BlendFactor cmbBlendFactor)
    => cmbBlendFactor switch {
        BlendFactor.Zero        => FinBlendFactor.ZERO,
        BlendFactor.One         => FinBlendFactor.ONE,
        BlendFactor.SourceColor => FinBlendFactor.SRC_COLOR,
        BlendFactor.OneMinusSourceColor => FinBlendFactor
            .ONE_MINUS_SRC_COLOR,
        BlendFactor.SourceAlpha => FinBlendFactor.SRC_ALPHA,
        BlendFactor.OneMinusSourceAlpha => FinBlendFactor
            .ONE_MINUS_SRC_ALPHA,
        BlendFactor.DestinationColor => FinBlendFactor.DST_COLOR,
        BlendFactor.OneMinusDestinationColor => FinBlendFactor
            .ONE_MINUS_DST_COLOR,
        BlendFactor.DestinationAlpha => FinBlendFactor.DST_ALPHA,
        BlendFactor.OneMinusDestinationAlpha => FinBlendFactor
            .ONE_MINUS_DST_ALPHA,
        BlendFactor.ConstantAlpha => FinBlendFactor.CONST_ALPHA,
        BlendFactor.OneMinusConstantAlpha => FinBlendFactor.ONE_MINUS_CONST_ALPHA,
        _ => throw new NotSupportedException(),
    };
}