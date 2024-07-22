using fin.math;
using fin.model;
using fin.shaders.glsl;
using fin.util.asserts;

namespace fin.ui.rendering.gl.material;

public static class GlMaterialShader {
  public static IGlMaterialShader FromMaterial(
      IReadOnlyModel model,
      IReadOnlyMaterial? material,
      IReadOnlyTextureTransformManager? textureTransformManager = null,
      IReadOnlyLighting? lighting = null)
    => material.GetShaderType() switch {
        FinShaderType.FIXED_FUNCTION => new GlFixedFunctionMaterialShader(
            model,
            Asserts.AsA<IReadOnlyFixedFunctionMaterial>(material),
            textureTransformManager,
            lighting),
        FinShaderType.TEXTURE => new GlTextureMaterialShader(model,
          Asserts.AsA<IReadOnlyTextureMaterial>(material),
          textureTransformManager,
          lighting),
        FinShaderType.COLOR => new GlColorMaterialShader(model,
          Asserts.AsA<IReadOnlyColorMaterial>(material),
          lighting),
        FinShaderType.STANDARD => new GlStandardMaterialShader(model,
          Asserts.AsA<IReadOnlyStandardMaterial>(material),
          textureTransformManager,
          lighting),
        FinShaderType.HIDDEN => new GlHiddenMaterialShader(
            model,
            lighting),
        FinShaderType.NULL => new GlNullMaterialShader(
            model,
            lighting),
    };
}