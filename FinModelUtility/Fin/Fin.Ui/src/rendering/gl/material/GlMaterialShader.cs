using fin.math;
using fin.model;
using fin.shaders.glsl;
using fin.util.asserts;

namespace fin.ui.rendering.gl.material {
  public static class GlMaterialShader {
    public static IGlMaterialShader FromMaterial(
        IModel model,
        IMaterial? material,
        IBoneTransformManager? boneTransformManager = null,
        IReadOnlyLighting? lighting = null)
      => material.GetShaderType() switch {
          FinShaderType.FIXED_FUNCTION => new GlFixedFunctionMaterialShader(
              model,
              Asserts.AsA<IFixedFunctionMaterial>(material),
              boneTransformManager,
              lighting),
          FinShaderType.TEXTURE => new GlTextureMaterialShader(model,
            Asserts.AsA<ITextureMaterial>(material),
            boneTransformManager,
            lighting),
          FinShaderType.COLOR => new GlColorMaterialShader(model,
            Asserts.AsA<IColorMaterial>(material),
            boneTransformManager,
            lighting),
          FinShaderType.STANDARD => new GlStandardMaterialShader(model,
            Asserts.AsA<IStandardMaterial>(material),
            boneTransformManager,
            lighting),
          FinShaderType.NULL => new GlNullMaterialShader(
              model,
              boneTransformManager,
              lighting),
      };
  }
}