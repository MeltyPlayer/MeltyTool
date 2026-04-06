using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using Assimp;

using fin.image;
using fin.model.util;

namespace fin.model.io.exporters.assimp.indirect;

public static class AssimpIndirectTextureFixer {
  public static void Fix(IReadOnlyModel model, Scene sc) {
    // Imports the textures
    var finTextures = new HashSet<IReadOnlyTexture>();
    foreach (var finMaterial in model.MaterialManager.All) {
      foreach (var finTexture in finMaterial.Textures) {
        finTextures.Add(finTexture);
      }
    }

    var originalMaterialOrder =
        sc.Materials.Select(material => material.Name).ToArray();

    sc.Textures.Clear();

    foreach (var finTexture in finTextures) {
      var format = finTexture.BestImageFormat;

      using var imageBytes = new MemoryStream();
      finTexture.Image.ExportToStream(imageBytes, format);

      var assTexture =
          new EmbeddedTexture(format.GetExtension()[1..],
                              imageBytes.ToArray(),
                              finTexture.Name) {
              Filename = finTexture.ValidFileName
          };

      sc.Textures.Add(assTexture);
    }

    // Need to keep order the same because Assimp references them by index.
    for (var m = 0; m < originalMaterialOrder.Length; ++m) {
      var originalMaterialName = originalMaterialOrder[m];
      var finMaterial =
          model.MaterialManager.All
               .FirstOrDefault(finMaterial
                                   => finMaterial.Name ==
                                      originalMaterialName);

      if (finMaterial == null) {
        continue;
      }

      var assMaterial = new Material { Name = finMaterial.Name };

      var primaryTexture = PrimaryTextureFinder.GetFor(finMaterial);
      if (primaryTexture != null) {
        assMaterial.AddMaterialTexture(CreateTextureSlot_(
            primaryTexture,
            TextureType.Diffuse));
      }

      if (finMaterial is IStandardMaterial standardMaterial &&
          standardMaterial.NormalTexture != null) {
        assMaterial.AddMaterialTexture(CreateTextureSlot_(
            standardMaterial.NormalTexture,
            TextureType.Normals));
      } else if (finMaterial is IFixedFunctionMaterial fixedFunctionMaterial &&
                 fixedFunctionMaterial.NormalTexture != null) {
        assMaterial.AddMaterialTexture(CreateTextureSlot_(
            fixedFunctionMaterial.NormalTexture,
            TextureType.Normals));
      }

      var extraTextures =
          finMaterial.Textures
                     .Where(finTexture => finTexture != primaryTexture)
                     .DistinctBy(finTexture => finTexture.ValidFileName)
                     .ToArray();

      for (var i = 0; i < extraTextures.Length; ++i) {
        var textureType = i switch {
            0 => TextureType.Emissive,
            1 => TextureType.Specular,
            2 => TextureType.Lightmap,
            _ => TextureType.Unknown
        };

        assMaterial.AddMaterialTexture(CreateTextureSlot_(
            extraTextures[i],
            textureType));
      }

      // Meshes should already have material indices set.
      sc.Materials[m] = assMaterial;
    }
  }

  private static TextureSlot CreateTextureSlot_(
      IReadOnlyTexture finTexture,
      TextureType textureType)
    => new() {
        FilePath = finTexture.ValidFileName,
        // TODO: FBX doesn't support mirror. Blegh
        WrapModeU = ConvertWrapMode_(finTexture.WrapModeU),
        WrapModeV = ConvertWrapMode_(finTexture.WrapModeV),
        TextureType = textureType,
        UVIndex = finTexture.UvIndex
    };

  private static TextureWrapMode ConvertWrapMode_(WrapMode wrapMode)
    => wrapMode switch {
        WrapMode.CLAMP         => TextureWrapMode.Clamp,
        WrapMode.REPEAT        => TextureWrapMode.Wrap,
        WrapMode.MIRROR_REPEAT => TextureWrapMode.Mirror,
        _ => throw new ArgumentOutOfRangeException(
            nameof(wrapMode),
            wrapMode,
            null)
    };
}
