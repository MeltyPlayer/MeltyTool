using fin.model;
using fin.util.asserts;

namespace fin.shaders.glsl;

public static class GlslMaterialExtensions {
  public static IShaderSourceGlsl ToShaderSource(
      this IReadOnlyMaterial? material,
      IReadOnlyModel model,
      bool useBoneMatrices) {
    var shaderRequirements
        = ShaderRequirements.FromModelAndMaterial(model, material);

    return material.GetShaderType() switch {
        FinShaderType.FIXED_FUNCTION
            => new FixedFunctionShaderSourceGlsl(
                model,
                Asserts.AsA<IFixedFunctionMaterial>(material),
                useBoneMatrices,
                shaderRequirements),
        FinShaderType.TEXTURE => new TextureShaderSourceGlsl(
            model,
            Asserts.AsA<ITextureMaterial>(material),
            useBoneMatrices,
            shaderRequirements),
        FinShaderType.COLOR => new ColorShaderSourceGlsl(
            model,
            Asserts.AsA<IColorMaterial>(material),
            useBoneMatrices,
            shaderRequirements),
        FinShaderType.STANDARD => new StandardShaderSourceGlsl(
            model,
            Asserts.AsA<IStandardMaterial>(material),
            useBoneMatrices,
            shaderRequirements),
        FinShaderType.HIDDEN => new HiddenShaderSourceGlsl(),
        FinShaderType.NULL => new NullShaderSourceGlsl(model,
          useBoneMatrices,
          shaderRequirements),
    };
  }
}