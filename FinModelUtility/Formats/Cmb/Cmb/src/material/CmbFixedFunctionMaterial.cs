using System;
using System.Linq;

using cmb.schema.cmb;

using fin.data.lazy;
using fin.image;
using fin.image.formats;
using fin.model;
using fin.util.image;

using SixLabors.ImageSharp.PixelFormats;

using FinBlendEquation = fin.model.BlendEquation;
using FinBlendFactor = fin.model.BlendFactor;
using CmbBlendMode = cmb.schema.cmb.BlendMode;
using CmbBlendEquation = cmb.schema.cmb.BlendEquation;
using CmbBlendFactor = cmb.schema.cmb.BlendFactor;
using CmbTextureMinFilter = cmb.schema.cmb.TextureMinFilter;
using CmbTextureMagFilter = cmb.schema.cmb.TextureMagFilter;
using FinTextureMinFilter = fin.model.TextureMinFilter;
using FinTextureMagFilter = fin.model.TextureMagFilter;

namespace cmb.material {
  public class CmbFixedFunctionMaterial {
    private const bool USE_FIXED_FUNCTION = true;
    private const bool USE_JANKY_TRANSPARENCY = false;

    public CmbFixedFunctionMaterial(
        IModel finModel,
        Cmb cmb,
        int materialIndex,
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
                  if (ImageUtil.GetTransparencyType(rawTextureImage) !=
                      ImageTransparencyType.OPAQUE || !USE_JANKY_TRANSPARENCY) {
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

                  /**
                  var cmbBorderColor =
                      texMapper.BorderColor;
                  finTexture.BorderColor = cmbBorderColor;
                   */

                  var cmbTexCoord = cmbMaterial.texCoords[i];

                  finTexture =
                      finModel.MaterialManager.CreateTexture(textureImage);
                  finTexture.Name = cmbTexture.name;
                  finTexture.WrapModeU = this.CmbToFinWrapMode(texMapper.wrapS);
                  finTexture.WrapModeV = this.CmbToFinWrapMode(texMapper.wrapT);
                  finTexture.MinFilter =
                      texMapper.minFilter switch {
                          CmbTextureMinFilter.Nearest =>
                              FinTextureMinFilter.NEAR,
                          CmbTextureMinFilter.Linear =>
                              FinTextureMinFilter.LINEAR,
                          CmbTextureMinFilter
                                  .NearestMipmapNearest =>
                              FinTextureMinFilter
                                  .NEAR_MIPMAP_NEAR,
                          CmbTextureMinFilter
                                  .LinearMipmapNearest =>
                              FinTextureMinFilter
                                  .LINEAR_MIPMAP_NEAR,
                          CmbTextureMinFilter
                                  .NearestMipmapLinear =>
                              FinTextureMinFilter
                                  .NEAR_MIPMAP_LINEAR,
                          CmbTextureMinFilter
                                  .LinearMipmapLinear =>
                              FinTextureMinFilter
                                  .LINEAR_MIPMAP_LINEAR,
                      };
                  finTexture.MagFilter =
                      texMapper.magFilter switch {
                          CmbTextureMagFilter.Nearest =>
                              FinTextureMagFilter.NEAR,
                          CmbTextureMagFilter.Linear =>
                              FinTextureMagFilter.LINEAR,
                      };
                  finTexture.LodBias = texMapper.lodBias;
                  finTexture.MinLod = texMapper.minLodBias;
                  finTexture.UvIndex = cmbTexCoord.coordinateIndex;

                  finTexture.UvType =
                      cmbTexCoord.mappingMethod ==
                      TextureMappingType.UvCoordinateMap
                          ? UvType.STANDARD
                          : UvType.SPHERICAL;
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
            new CmbCombinerGenerator(cmbMaterial, finMaterial);

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
              AlphaOp.Or,
              cmbMaterial.alphaTestFunction switch {
                  TestFunc.Always   => AlphaCompareType.Always,
                  TestFunc.Equal    => AlphaCompareType.Equal,
                  TestFunc.Gequal   => AlphaCompareType.GEqual,
                  TestFunc.Greater  => AlphaCompareType.Greater,
                  TestFunc.Never    => AlphaCompareType.Never,
                  TestFunc.Less     => AlphaCompareType.Less,
                  TestFunc.Lequal   => AlphaCompareType.LEqual,
                  TestFunc.Notequal => AlphaCompareType.NEqual,
              },
              cmbMaterial.alphaTestReferenceValue,
              AlphaCompareType.Never,
              0);
        }

        // TODO: not right
        switch (cmbMaterial.blendMode) {
          case CmbBlendMode.BlendNone: {
            finMaterial.SetBlending(
                FinBlendEquation.ADD,
                FinBlendFactor.ONE,
                FinBlendFactor.ZERO,
                LogicOp.UNDEFINED);
            break;
          }
          case CmbBlendMode.Blend: {
            finMaterial.SetBlending(
                CmbBlendEquationToFin(cmbMaterial.colorEquation),
                CmbBlendFactorToFin(cmbMaterial.colorSrcFunc),
                CmbBlendFactorToFin(cmbMaterial.colorDstFunc),
                LogicOp.UNDEFINED);
            break;
          }
          case CmbBlendMode.BlendSeparate: {
            finMaterial.SetBlendingSeparate(
                CmbBlendEquationToFin(cmbMaterial.colorEquation),
                CmbBlendFactorToFin(cmbMaterial.colorSrcFunc),
                CmbBlendFactorToFin(cmbMaterial.colorDstFunc),
                CmbBlendEquationToFin(cmbMaterial.alphaEquation),
                CmbBlendFactorToFin(cmbMaterial.alphaSrcFunc),
                CmbBlendFactorToFin(cmbMaterial.alphaDstFunc),
                LogicOp.UNDEFINED);
            break;
          }
          case CmbBlendMode.LogicalOp: break;
          default:                     throw new ArgumentOutOfRangeException();
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
        finMaterial.DepthMode = cmbMaterial.depthTestEnabled switch {
            true => cmbMaterial.depthWriteEnabled
                ? DepthMode.USE_DEPTH_BUFFER
                : DepthMode.SKIP_WRITE_TO_DEPTH_BUFFER,
            false => DepthMode.IGNORE_DEPTH_BUFFER,
        };
      }

      this.Material.Name = $"material{materialIndex}";
      this.Material.CullingMode = cmbMaterial.faceCulling switch {
          CullMode.FrontAndBack => CullingMode.SHOW_BOTH,
          CullMode.Front        => CullingMode.SHOW_FRONT_ONLY,
          CullMode.BackFace     => CullingMode.SHOW_BACK_ONLY,
          CullMode.Never        => CullingMode.SHOW_NEITHER,
      };
    }

    public IMaterial Material { get; }

    public WrapMode CmbToFinWrapMode(TextureWrapMode cmbMode)
      => cmbMode switch {
          TextureWrapMode.ClampToBorder => WrapMode.CLAMP,
          TextureWrapMode.Repeat        => WrapMode.REPEAT,
          TextureWrapMode.ClampToEdge   => WrapMode.CLAMP,
          TextureWrapMode.Mirror        => WrapMode.MIRROR_REPEAT,
      };

    public FinBlendEquation CmbBlendEquationToFin(
        CmbBlendEquation cmbBlendEquation)
      => cmbBlendEquation switch {
          CmbBlendEquation.FuncAdd      => FinBlendEquation.ADD,
          CmbBlendEquation.FuncSubtract => FinBlendEquation.SUBTRACT,
          CmbBlendEquation.FuncReverseSubtract => FinBlendEquation
              .REVERSE_SUBTRACT,
          CmbBlendEquation.Min => FinBlendEquation.MIN,
          CmbBlendEquation.Max => FinBlendEquation.MAX,
          _ => throw new ArgumentOutOfRangeException(
              nameof(cmbBlendEquation),
              cmbBlendEquation,
              null)
      };

    public FinBlendFactor CmbBlendFactorToFin(CmbBlendFactor cmbBlendFactor)
      => cmbBlendFactor switch {
          CmbBlendFactor.Zero        => FinBlendFactor.ZERO,
          CmbBlendFactor.One         => FinBlendFactor.ONE,
          CmbBlendFactor.SourceColor => FinBlendFactor.SRC_COLOR,
          CmbBlendFactor.OneMinusSourceColor => FinBlendFactor
              .ONE_MINUS_SRC_COLOR,
          CmbBlendFactor.SourceAlpha => FinBlendFactor.SRC_ALPHA,
          CmbBlendFactor.OneMinusSourceAlpha => FinBlendFactor
              .ONE_MINUS_SRC_ALPHA,
          CmbBlendFactor.DestinationAlpha => FinBlendFactor.DST_ALPHA,
          CmbBlendFactor.OneMinusDestinationAlpha => FinBlendFactor
              .ONE_MINUS_DST_ALPHA,
      };
  }
}