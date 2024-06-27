using System;
using System.Linq;

using fin.util.image;

namespace fin.model.util;

public static class PrimaryTextureFinder {
  public static IReadOnlyTexture? GetFor(IReadOnlyMaterial material) {
    if (material is IReadOnlyNullMaterial) {
      return null;
    }

    if (material is IReadOnlyFixedFunctionMaterial fixedFunctionMaterial) {
      return PrimaryTextureFinder.GetFor(fixedFunctionMaterial);
    }

    if (material is IReadOnlyTextureMaterial textureMaterial) {
      return PrimaryTextureFinder.GetFor(textureMaterial);
    }

    if (material is IReadOnlyStandardMaterial standardMaterial) {
      return PrimaryTextureFinder.GetFor(standardMaterial);
    }

    throw new NotImplementedException();
  }

  public static IReadOnlyTexture? GetFor(IReadOnlyTextureMaterial material)
    => material.Texture;

  public static IReadOnlyTexture? GetFor(
      IReadOnlyFixedFunctionMaterial material) {
    var equations = material.Equations;

    var textures = material.Textures;

    // TODO: Use some kind of priority class

    var compiledTexture = material.CompiledTexture;
    if (compiledTexture != null) {
      return compiledTexture;
    }

    var prioritizedTextures =
        textures
            // Sort by UV type, "normal" first
            .OrderByDescending(
                texture => texture.ColorType == ColorType.COLOR)
            .ThenByDescending(
                texture => TransparencyTypeUtil.GetTransparencyType(texture.Image) ==
                           TransparencyType.OPAQUE)
            .ToArray();

    if (prioritizedTextures.Length > 0) {
      // TODO: First or last?
      return prioritizedTextures[0];
    }

    return material.Textures.LastOrDefault((IReadOnlyTexture?) null);

    // TODO: Prioritize textures w/ color rather than intensity
    // TODO: Prioritize textures w/ standard texture sets
  }

  public static IReadOnlyTexture? GetFor(IReadOnlyStandardMaterial material)
    => material.DiffuseTexture ?? material.AmbientOcclusionTexture;
}