using System;

using fin.image.util;
using fin.language.equations.fixedFunction.util;

namespace fin.model.util;

public static class MaterialTransparencyTypeUtil {
  public static TransparencyType GetTransparencyType(
      this IReadOnlyMaterial material) {
    // TODO: Anything else to handle here?
    // First off, handle any blend equations:
    {
      if (material.ColorSrcFactor is BlendFactor.ONE &&
          material.ColorDstFactor is BlendFactor.ZERO) {
        return TransparencyType.OPAQUE;
      }

      // Additive blend mode.
      if (material.ColorSrcFactor is BlendFactor.SRC_ALPHA &&
          material.ColorDstFactor is BlendFactor.ONE) {
        return TransparencyType.TRANSPARENT;
      }

      // Subtractive blend modes depend on what's already been drawn, so they
      // need to be rendered after opaque geometry.
      if (material.ColorBlendEquation is BlendEquation.SUBTRACT
                                         or BlendEquation.REVERSE_SUBTRACT) {
        return TransparencyType.TRANSPARENT;
      }
    }

    // TODO: Detect this from the model
    var hasTransparentVertexColors = false;

    switch (material) {
      // Always opaque
      case IReadOnlyHiddenMaterial: {
        return TransparencyType.OPAQUE;
      }
      case IReadOnlyShaderMaterial: {
        return TransparencyType.TRANSPARENT;
      }
      case IReadOnlyNullMaterial: {
        return hasTransparentVertexColors
            ? TransparencyType.TRANSPARENT
            : TransparencyType.OPAQUE;
      }
      case IReadOnlyColorMaterial colorMaterial: {
        return hasTransparentVertexColors || colorMaterial.Color.A < 255
            ? TransparencyType.TRANSPARENT
            : TransparencyType.OPAQUE;
      }
      case IReadOnlyTextureMaterial textureMaterial: {
        var textureTransparencyType = textureMaterial.Texture.TransparencyType;
        return hasTransparentVertexColors ||
               (textureMaterial.DiffuseColor?.A ?? 255) < 255 ||
               textureTransparencyType is TransparencyType.TRANSPARENT
            ? TransparencyType.TRANSPARENT
            : textureTransparencyType;
      }
      case IReadOnlyStandardMaterial standardMaterial: {
        var textureTransparencyType
            = standardMaterial.DiffuseTexture?.TransparencyType ??
              TransparencyType.OPAQUE;
        return hasTransparentVertexColors ||
               textureTransparencyType is TransparencyType.TRANSPARENT
            ? TransparencyType.TRANSPARENT
            : textureTransparencyType;
      }
      case IReadOnlyFixedFunctionMaterial fixedFunctionMaterial: {
        var equations = fixedFunctionMaterial.Equations;
        var outputAlpha
            = equations.ScalarOutputs[FixedFunctionSource.OUTPUT_ALPHA];
        var isOne = outputAlpha.IsOne();
        return isOne
            ? TransparencyType.OPAQUE
            : TransparencyType.TRANSPARENT;
      }
      default: throw new ArgumentOutOfRangeException(nameof(material));
    }
  }
}