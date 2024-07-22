using fin.math;
using fin.model;
using fin.shaders.glsl;
using fin.util.asserts;

namespace fin.ui.rendering.gl.material;

public static class GlMaterialShader {
  public static IGlMaterialShader FromMaterial(
      IReadOnlyModel model,
      IReadOnlyMaterial? material,
      IReadOnlyTextureTransformManager? textureTransformManager = null)
    => material.GetShaderType() switch {
        FinShaderType.FIXED_FUNCTION => new GlFixedFunctionMaterialShader(
            model,
            Asserts.AsA<IReadOnlyFixedFunctionMaterial>(material),
            textureTransformManager),
        FinShaderType.TEXTURE => new GlTextureMaterialShader(model,
          Asserts.AsA<IReadOnlyTextureMaterial>(material),
          textureTransformManager),
        FinShaderType.COLOR => new GlColorMaterialShader(model,
          Asserts.AsA<IReadOnlyColorMaterial>(material)),
        FinShaderType.STANDARD => new GlStandardMaterialShader(model,
          Asserts.AsA<IReadOnlyStandardMaterial>(material),
          textureTransformManager),
        FinShaderType.HIDDEN => new GlHiddenMaterialShader(model),
        FinShaderType.NULL => new GlNullMaterialShader(model),
    };
}