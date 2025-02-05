using fin.math;
using fin.model;
using fin.shaders.glsl;
using fin.util.asserts;

namespace fin.ui.rendering.gl.material;

public static class GlMaterialShader {
  public static IGlMaterialShader FromMaterial(
      IReadOnlyModel model,
      IModelRequirements modelRequirements,
      IReadOnlyMaterial? material,
      IReadOnlyTextureTransformManager? textureTransformManager = null)
    => material.GetShaderType() switch {
        FinShaderType.FIXED_FUNCTION => new GlFixedFunctionMaterialShader(
            model,
            modelRequirements,
            Asserts.AsA<IReadOnlyFixedFunctionMaterial>(material),
            textureTransformManager),
        FinShaderType.TEXTURE => new GlTextureMaterialShader(model,
          modelRequirements,
          Asserts.AsA<IReadOnlyTextureMaterial>(material),
          textureTransformManager),
        FinShaderType.COLOR => new GlColorMaterialShader(model,
          modelRequirements,
          Asserts.AsA<IReadOnlyColorMaterial>(material)),
        FinShaderType.SHADER => new GlShaderMaterialShader(model,
          modelRequirements,
          Asserts.AsA<IReadOnlyShaderMaterial>(material)),
        FinShaderType.STANDARD => new GlStandardMaterialShader(model,
          modelRequirements,
          Asserts.AsA<IReadOnlyStandardMaterial>(material),
          textureTransformManager),
        FinShaderType.HIDDEN => new GlHiddenMaterialShader(
            model,
            modelRequirements),
        FinShaderType.NULL
            => new GlNullMaterialShader(model, modelRequirements),
    };
}