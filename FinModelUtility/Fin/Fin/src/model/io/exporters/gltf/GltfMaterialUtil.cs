using System;
using System.Drawing;
using System.Numerics;

using fin.color;
using fin.image.util;
using fin.math.matrix.four;

using SharpGLTF.Materials;

namespace fin.model.io.exporters.gltf;

public static class GltfMaterialUtil {
  public static MaterialBuilder WithMetallicRoughnessIfLit(
      this MaterialBuilder gltfMaterialBuilder,
      IReadOnlyMaterial finMaterial)
    => gltfMaterialBuilder.WithMetallicRoughnessIfLit(finMaterial, out _);

  public static MaterialBuilder WithMetallicRoughnessIfLit(
      this MaterialBuilder gltfMaterialBuilder,
      IReadOnlyMaterial finMaterial,
      out bool usesLighting,
      bool usesDiffuseLighting = true,
      bool usesSpecularLighting = true,
      bool usesAmbientLighting = true) {
    if (finMaterial.IgnoreLights ||
        (!usesDiffuseLighting &&
         !usesSpecularLighting &&
         !usesAmbientLighting)) {
      usesLighting = false;
      return gltfMaterialBuilder.WithUnlitShader();
    }

    // Shamelessly stolen from:
    // https://computergraphics.stackexchange.com/questions/1515/what-is-the-accepted-method-of-converting-shininess-to-roughness-and-vice-versa
    var roughness = !usesSpecularLighting
        ? 1
        : MathF.Sqrt(2 / (finMaterial.Shininess + 2));

    usesLighting = true;
    return gltfMaterialBuilder.WithMetallicRoughnessShader()
                              // Fixed-function materials are dielectric unless
                              // they explicitly carry PBR metalness data.
                              .WithMetallicRoughness(0, roughness);
  }

  public static MaterialBuilder WithBaseColor(
      this MaterialBuilder gltfMaterialBuilder,
      Color color) {
    var baseColor = color.AsVector4();
    if (!baseColor.IsRoughly(Vector4.One)) {
      gltfMaterialBuilder.WithBaseColor(baseColor);
    }

    return gltfMaterialBuilder;
  }

  public static MaterialBuilder WithAlpha(
      this MaterialBuilder gltfMaterialBuilder,
      TransparencyType transparencyType)
    => gltfMaterialBuilder.WithAlpha(
        transparencyType switch {
            TransparencyType.OPAQUE => AlphaMode.OPAQUE,
            TransparencyType.MASK => AlphaMode.MASK,
            TransparencyType.TRANSPARENT => AlphaMode.BLEND,
            _ => throw new ArgumentOutOfRangeException()
        });
}
