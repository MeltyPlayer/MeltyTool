using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;

using fin.language.equations.fixedFunction;
using CommunityToolkit.HighPerformance.Helpers;

using fin.image;
using fin.model.util;
using fin.util.image;

using SharpGLTF.Materials;
using SharpGLTF.Memory;
using SharpGLTF.Schema2;

using AlphaMode = SharpGLTF.Materials.AlphaMode;

namespace fin.model.io.exporters.gltf {
  public class GltfMaterialBuilder {
    private readonly struct Fin2GltfImageConverter : IAction {
      private readonly IImage[] finImages_;
      private readonly IDictionary<IImage, MemoryImage> gltfImageByFinImage_;

      public Fin2GltfImageConverter(IImage[] finImages,
                                    IDictionary<IImage, MemoryImage>
                                        gltfImageByFinImage) {
        this.finImages_ = finImages;
        this.gltfImageByFinImage_ = gltfImageByFinImage;
      }

      public void Invoke(int i) {
        var finImage = this.finImages_[i];

        using var imageStream = new MemoryStream();
        finImage.ExportToStream(imageStream, LocalImageFormat.PNG);

        this.gltfImageByFinImage_[finImage] =
            new MemoryImage(imageStream.ToArray());
      }
    }

    public IDictionary<IMaterial, Material> GetMaterials(
        ModelRoot gltfModelRoot,
        IMaterialManager finMaterialManager)
      => this.ConvertMaterials_(finMaterialManager)
             .ToDictionary(tuple => tuple.Item1,
                           tuple => gltfModelRoot.CreateMaterial(tuple.Item2));

    public IDictionary<IMaterial, MaterialBuilder> GetMaterialBuilders(
        IMaterialManager finMaterialManager)
      => this.ConvertMaterials_(finMaterialManager)
             .ToDictionary(tuple => tuple.Item1, tuple => tuple.Item2);

    private IEnumerable<(IMaterial, MaterialBuilder)> ConvertMaterials_(
        IMaterialManager finMaterialManager) {
      var finImages = finMaterialManager.Textures
                                        .Select(texture => texture.Image)
                                        .Distinct()
                                        .ToArray();
      var gltfImageByFinImage = new ConcurrentDictionary<IImage, MemoryImage>();
      ParallelHelper.For(0,
                         finImages.Length,
                         new Fin2GltfImageConverter(
                             finImages,
                             gltfImageByFinImage));

      // TODO: Update this if GLTF is ever extended...
      return finMaterialManager.All.Select(finMaterial => {
        var gltfMaterialBuilder
            = new MaterialBuilder(finMaterial.Name)
              .WithDoubleSide(finMaterial.CullingMode switch {
                  CullingMode.SHOW_FRONT_ONLY => false,
                  // Darn, guess we can't support this.
                  CullingMode.SHOW_BACK_ONLY => true,
                  CullingMode.SHOW_BOTH      => true,
                  // Darn, guess we can't support this either.
                  CullingMode.SHOW_NEITHER => false,
                  _ => throw new ArgumentOutOfRangeException()
              });

        switch (finMaterial) {
          case IStandardMaterial standardMaterial: {
            gltfMaterialBuilder.WithSpecularGlossinessShader()
                               .WithSpecularGlossiness(new Vector3(0), 0);

            var diffuseTexture = standardMaterial.DiffuseTexture;
            if (diffuseTexture != null) {
              gltfMaterialBuilder.UseChannel(KnownChannel.Diffuse)
                                 .UseTexture(diffuseTexture,
                                             gltfImageByFinImage[
                                                 diffuseTexture.Image]);
            }

            var normalTexture = standardMaterial.NormalTexture;
            if (normalTexture != null) {
              gltfMaterialBuilder.UseChannel(KnownChannel.Normal)
                                 .UseTexture(normalTexture,
                                             gltfImageByFinImage
                                                 [normalTexture.Image]);
            }

            var emissiveTexture = standardMaterial.EmissiveTexture;
            if (emissiveTexture != null) {
              gltfMaterialBuilder.UseChannel(KnownChannel.Emissive)
                                 .UseTexture(emissiveTexture,
                                             gltfImageByFinImage[
                                                 emissiveTexture.Image]);
            }

            /*var specularTexture = standardMaterial.SpecularTexture;
            if (specularTexture != null) {
              gltfMaterial.WithSpecularGlossiness(
                  GltfModelExporter.GetGltfImageFromFinTexture_(
                      specularTexture), new Vector3(.1f), .1f);
            }*/

            var ambientOcclusionTexture =
                standardMaterial.AmbientOcclusionTexture;
            if (ambientOcclusionTexture != null) {
              gltfMaterialBuilder
                  .UseChannel(KnownChannel.Occlusion)
                  .UseTexture(ambientOcclusionTexture,
                              gltfImageByFinImage[
                                  ambientOcclusionTexture.Image]);
            }

            break;
          }
          case IFixedFunctionMaterial fixedFunctionMaterial: {
            var equations = fixedFunctionMaterial.Equations;
            var usesSpecular = equations.DoOutputsDependOn(
                Enumerable
                    .Range(0, MaterialConstants.MAX_LIGHTS)
                    .SelectMany<int, FixedFunctionSource>(i => [
                        FixedFunctionSource.LIGHT_SPECULAR_COLOR_0 + i,
                        FixedFunctionSource.LIGHT_SPECULAR_ALPHA_0 + i
                    ])
                    .Concat([
                        FixedFunctionSource.LIGHT_SPECULAR_COLOR_MERGED,
                        FixedFunctionSource.LIGHT_SPECULAR_ALPHA_MERGED
                    ])
                    .ToArray());
            var usesDiffuse = equations.DoOutputsDependOn(
                Enumerable
                    .Range(0, MaterialConstants.MAX_LIGHTS)
                    .SelectMany<int, FixedFunctionSource>(i => [
                        FixedFunctionSource.LIGHT_DIFFUSE_COLOR_0 + i,
                        FixedFunctionSource.LIGHT_DIFFUSE_ALPHA_0 + i
                    ])
                    .Concat([
                        FixedFunctionSource.LIGHT_DIFFUSE_COLOR_MERGED,
                        FixedFunctionSource.LIGHT_DIFFUSE_ALPHA_MERGED
                    ])
                    .ToArray());

            KnownChannel mainTextureChannel;
            if (usesSpecular) {
              // TODO: Get specular color
              gltfMaterialBuilder
                  .WithSpecularGlossinessShader()
                  .WithSpecularGlossiness(null,
                                          fixedFunctionMaterial.Shininess);
              mainTextureChannel = KnownChannel.Diffuse;
            } else if (usesDiffuse) {
              // TODO: Get diffuse color
              gltfMaterialBuilder.WithMetallicRoughnessShader();
              mainTextureChannel = KnownChannel.BaseColor;
            } else {
              gltfMaterialBuilder.WithUnlitShader();
              mainTextureChannel = KnownChannel.BaseColor;
            }

            var texture = PrimaryTextureFinder.GetFor(finMaterial);
            if (texture != null) {
              var alphaMode = texture.TransparencyType switch {
                  ImageTransparencyType.OPAQUE => AlphaMode.OPAQUE,
                  ImageTransparencyType.MASK => AlphaMode.MASK,
                  ImageTransparencyType.TRANSPARENT => AlphaMode.BLEND,
                  _ => throw new ArgumentOutOfRangeException()
              };
              gltfMaterialBuilder.WithAlpha(alphaMode);

              gltfMaterialBuilder
                  .UseChannel(mainTextureChannel)
                  .UseTexture(texture, gltfImageByFinImage[texture.Image]);
            }

            break;
          }
          default: {
            var texture = PrimaryTextureFinder.GetFor(finMaterial);
            if (texture != null) {
              var alphaMode = texture.TransparencyType switch {
                  ImageTransparencyType.OPAQUE => AlphaMode.OPAQUE,
                  ImageTransparencyType.MASK => AlphaMode.MASK,
                  ImageTransparencyType.TRANSPARENT => AlphaMode.BLEND,
                  _ => throw new ArgumentOutOfRangeException()
              };
              gltfMaterialBuilder.WithAlpha(alphaMode);

              gltfMaterialBuilder
                  .WithSpecularGlossinessShader()
                  .WithSpecularGlossiness(null, 0)
                  .UseChannel(KnownChannel.Diffuse)
                  .UseTexture(texture, gltfImageByFinImage[texture.Image]);
            }

            break;
          }
        }

        return (finMaterial, gltfMaterial: gltfMaterialBuilder);
      });
    }
  }
}