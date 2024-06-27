using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using Assimp;

using fin.image;
using fin.model.util;

namespace fin.model.io.exporters.assimp.indirect;

public class AssimpIndirectTextureFixer {
  public void Fix(IReadOnlyModel model, Scene sc) {
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

      var assMaterial = new Material {Name = finMaterial.Name};

      var finTexture = PrimaryTextureFinder.GetFor(finMaterial);
      if (finTexture != null) {
        var assTextureSlot = new TextureSlot {
            FilePath = finTexture.ValidFileName,
            // TODO: FBX doesn't support mirror. Blegh
            WrapModeU = this.ConvertWrapMode_(finTexture.WrapModeU),
            WrapModeV = this.ConvertWrapMode_(finTexture.WrapModeV)
        };

        assTextureSlot.TextureType = TextureType.Diffuse;
        assTextureSlot.UVIndex = finTexture.UvIndex;

        assMaterial.AddMaterialTexture(assTextureSlot);
      }

      // Meshes should already have material indices set.
      sc.Materials[m] = assMaterial;
    }
  }

  private TextureWrapMode ConvertWrapMode_(WrapMode wrapMode)
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