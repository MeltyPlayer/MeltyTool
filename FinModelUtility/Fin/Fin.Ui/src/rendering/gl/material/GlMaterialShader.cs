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
      IReadOnlyTextureTransformManager textureTransformManager,
      IReadOnlyTextureSwapManager textureSwapManager)
    => material.GetShaderType() switch {
        FinShaderType.FIXED_FUNCTION => new GlFixedFunctionMaterialShader(
            model,
            modelRequirements,
            Asserts.AsA<IReadOnlyFixedFunctionMaterial>(material),
            textureTransformManager,
            textureSwapManager),
        FinShaderType.TEXTURE => new GlTextureMaterialShader(model,
          modelRequirements,
          Asserts.AsA<IReadOnlyTextureMaterial>(material),
          textureTransformManager,
          textureSwapManager),
        FinShaderType.COLOR => new GlColorMaterialShader(model,
          modelRequirements,
          Asserts.AsA<IReadOnlyColorMaterial>(material),
          textureTransformManager,
          textureSwapManager),
        FinShaderType.SHADER => new GlShaderMaterialShader(model,
          modelRequirements,
          Asserts.AsA<IReadOnlyShaderMaterial>(material),
          textureTransformManager,
          textureSwapManager),
        FinShaderType.STANDARD => new GlStandardMaterialShader(model,
          modelRequirements,
          Asserts.AsA<IReadOnlyStandardMaterial>(material),
          textureTransformManager,
          textureSwapManager),
        FinShaderType.HIDDEN => new GlHiddenMaterialShader(
            model,
            modelRequirements,
            textureTransformManager,
            textureSwapManager),
        FinShaderType.NULL
            => new GlNullMaterialShader(
                model,
                modelRequirements,
                textureTransformManager,
                textureSwapManager),
        _ => throw new ArgumentOutOfRangeException()
    };
}