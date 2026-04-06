using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;

using CommunityToolkit.HighPerformance.Helpers;

using fin.image;
using fin.model.util;
using fin.image.util;

using SharpGLTF.Materials;
using SharpGLTF.Memory;
using SharpGLTF.Schema2;

using AlphaMode = SharpGLTF.Materials.AlphaMode;

namespace fin.model.io.exporters.gltf;

public static class GltfMaterialBuilder {
  private readonly struct Fin2GltfImageConverter(
      IReadOnlyImage[] finImages,
      IDictionary<IReadOnlyImage, MemoryImage> gltfImageByFinImage)
      : IAction {
    public void Invoke(int i) {
      var finImage = finImages[i];

      using var imageStream = new MemoryStream();
      finImage.ExportToStream(imageStream, LocalImageFormat.PNG);

      gltfImageByFinImage[finImage] =
          new MemoryImage(imageStream.ToArray());
    }
  }

  public static IDictionary<IReadOnlyMaterial, Material> GetMaterials(
      ModelRoot gltfModelRoot,
      IReadOnlyMaterialManager finMaterialManager)
    => ConvertMaterials_(finMaterialManager)
           .ToDictionary(tuple => tuple.Item1,
                         tuple => gltfModelRoot.CreateMaterial(tuple.Item2));

  public static IDictionary<IReadOnlyMaterial, MaterialBuilder> GetMaterialBuilders(
      IReadOnlyMaterialManager finMaterialManager)
    => ConvertMaterials_(finMaterialManager)
           .ToDictionary(tuple => tuple.Item1, tuple => tuple.Item2);

  private static IEnumerable<(IReadOnlyMaterial, MaterialBuilder)> ConvertMaterials_(
      IReadOnlyMaterialManager finMaterialManager) {
    var finImages = finMaterialManager.Textures
                                      .Select(texture => texture.Image)
                                      .Distinct()
                                      .ToArray();
    var gltfImageByFinImage
        = new ConcurrentDictionary<IReadOnlyImage, MemoryImage>();
    ParallelHelper.For(0,
                       finImages.Length,
                       new Fin2GltfImageConverter(
                           finImages,
                           gltfImageByFinImage));

    // TODO: Update this if GLTF is ever extended...
    return finMaterialManager.All.Select(
        finMaterial => {
          var gltfMaterialBuilder
              = new MaterialBuilder(
                      finMaterial.Name)
                  .WithDoubleSide(
                      finMaterial.CullingMode
                          switch {
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
              gltfMaterialBuilder
                  .WithSpecularGlossinessShader()
                  .WithSpecularGlossiness(
                      new Vector3(0),
                      0);

              var diffuseTexture = standardMaterial.DiffuseTexture;
              if (diffuseTexture != null) {
                gltfMaterialBuilder
                    .UseChannel(KnownChannel.Diffuse)
                    .UseTexture(diffuseTexture,
                                gltfImageByFinImage[diffuseTexture.Image]);
              }

              // TODO: Include diffuse color

              var normalTexture = standardMaterial.NormalTexture;
              if (normalTexture != null) {
                gltfMaterialBuilder.UseChannel(KnownChannel.Normal)
                                   .UseTexture(normalTexture,
                                               gltfImageByFinImage
                                                   [normalTexture.Image]);
              }

              var emissiveTexture = standardMaterial.EmissiveTexture;
              if (emissiveTexture != null) {
                gltfMaterialBuilder
                    .UseChannel(KnownChannel.Emissive)
                    .UseTexture(emissiveTexture,
                                gltfImageByFinImage[emissiveTexture.Image]);
              }

              /*var specularTexture = standardMaterial.SpecularTexture;
              if (specularTexture != null) {
                gltfMaterial.WithSpecularGlossiness(
                    GltfModelExporter.GetGltfImageFromFinTexture_(
                        specularTexture), new Vector3(.1f), .1f);
              }*/

              var ambientOcclusionTexture
                  = standardMaterial.AmbientOcclusionTexture;
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
              var usesSpecular
                  = equations
                      .DoOutputsDependOn(
                          Enumerable
                              .Range(0,
                                     MaterialConstants
                                         .MAX_LIGHTS)
                              .SelectMany<int
                                  , FixedFunctionSource>(
                                  i => [
                                      FixedFunctionSource
                                          .LIGHT_SPECULAR_COLOR_0 +
                                      i,
                                      FixedFunctionSource
                                          .LIGHT_SPECULAR_ALPHA_0 +
                                      i
                                  ])
                              .Concat([
                                  FixedFunctionSource
                                      .LIGHT_SPECULAR_COLOR_MERGED,
                                  FixedFunctionSource
                                      .LIGHT_SPECULAR_ALPHA_MERGED
                              ])
                              .ToArray());
              var usesDiffuse
                  = equations
                      .DoOutputsDependOn(
                          Enumerable
                              .Range(0, MaterialConstants.MAX_LIGHTS)
                              .SelectMany<int, FixedFunctionSource>(i => [
                                  FixedFunctionSource.LIGHT_DIFFUSE_COLOR_0 +
                                  i,
                                  FixedFunctionSource.LIGHT_DIFFUSE_ALPHA_0 +
                                  i
                              ])
                              .Concat([
                                  FixedFunctionSource
                                      .LIGHT_DIFFUSE_COLOR_MERGED,
                                  FixedFunctionSource
                                      .LIGHT_DIFFUSE_ALPHA_MERGED
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

              var texture
                  = PrimaryTextureFinder
                      .GetFor(finMaterial);
              if (texture != null) {
                var alphaMode = texture.TransparencyType
                    switch {
                        TransparencyType.OPAQUE => AlphaMode.OPAQUE,
                        TransparencyType.MASK => AlphaMode.MASK,
                        TransparencyType.TRANSPARENT => AlphaMode.BLEND,
                        _ => throw new ArgumentOutOfRangeException()
                    };
                gltfMaterialBuilder.WithAlpha(alphaMode);

                gltfMaterialBuilder
                    .UseChannel(mainTextureChannel)
                    .UseTexture(texture, gltfImageByFinImage[texture.Image]);
              }

              var normalTexture = fixedFunctionMaterial.NormalTexture;
              if (normalTexture != null) {
                gltfMaterialBuilder
                    .UseChannel(KnownChannel.Normal)
                    .UseTexture(normalTexture,
                                gltfImageByFinImage[normalTexture.Image]);
              }

              var extraTextures =
                  fixedFunctionMaterial.Textures
                                       .Where(extraTexture =>
                                                  extraTexture != texture &&
                                                  extraTexture != normalTexture)
                                       .DistinctBy(extraTexture => extraTexture.Name)
                                       .ToArray();

              if (extraTextures.Length > 0) {
                gltfMaterialBuilder
                    .UseChannel(KnownChannel.Emissive)
                    .UseTexture(extraTextures[0],
                                gltfImageByFinImage[extraTextures[0].Image]);
              }

              if (extraTextures.Length > 1) {
                gltfMaterialBuilder
                    .UseChannel(KnownChannel.Occlusion)
                    .UseTexture(extraTextures[1],
                                gltfImageByFinImage[extraTextures[1].Image]);
              }

              break;
            }
            default: {
              var texture = PrimaryTextureFinder.GetFor(finMaterial);
              if (texture != null) {
                var alphaMode = texture.TransparencyType
                    switch {
                        TransparencyType.OPAQUE => AlphaMode.OPAQUE,
                        TransparencyType.MASK => AlphaMode.MASK,
                        TransparencyType.TRANSPARENT => AlphaMode.BLEND,
                        _ => throw new ArgumentOutOfRangeException()
                    };
                gltfMaterialBuilder.WithAlpha(alphaMode);

                gltfMaterialBuilder
                    .WithSpecularGlossinessShader()
                    .WithSpecularGlossiness(null, 0)
                    .UseChannel(KnownChannel.Diffuse)
                    .UseTexture(
                        texture,
                        gltfImageByFinImage[texture.Image]);

                var extraTextures =
                    finMaterial.Textures
                               .Where(extraTexture => extraTexture != texture)
                               .DistinctBy(extraTexture => extraTexture.Name)
                               .ToArray();

                if (extraTextures.Length > 0) {
                  gltfMaterialBuilder
                      .UseChannel(KnownChannel.Emissive)
                      .UseTexture(extraTextures[0],
                                  gltfImageByFinImage[extraTextures[0].Image]);
                }

                if (extraTextures.Length > 1) {
                  gltfMaterialBuilder
                      .UseChannel(KnownChannel.Occlusion)
                      .UseTexture(extraTextures[1],
                                  gltfImageByFinImage[extraTextures[1].Image]);
                }
              }

              break;
            }
          }

          return (finMaterial, gltfMaterial: gltfMaterialBuilder);
        });
  }
}
