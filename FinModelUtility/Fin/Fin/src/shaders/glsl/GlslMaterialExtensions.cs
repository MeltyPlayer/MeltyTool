using fin.model;
using fin.shaders.glsl.source;
using fin.util.asserts;

namespace fin.shaders.glsl;

public static class GlslMaterialExtensions {
  public static IShaderSourceGlsl ToShaderSource(
      this IReadOnlyMaterial? material,
      IReadOnlyModel model) {
    var shaderRequirements
        = ShaderRequirements.FromModelAndMaterial(model, material);

    return material.GetShaderType() switch {
        FinShaderType.FIXED_FUNCTION
            => new FixedFunctionShaderSourceGlsl(
                model,
                Asserts.AsA<IFixedFunctionMaterial>(material),
                shaderRequirements),
        FinShaderType.TEXTURE => new TextureShaderSourceGlsl(
            model,
            Asserts.AsA<ITextureMaterial>(material),
            shaderRequirements),
        FinShaderType.COLOR => new ColorShaderSourceGlsl(
            model,
            Asserts.AsA<IColorMaterial>(material),
            shaderRequirements),
        FinShaderType.STANDARD => new StandardShaderSourceGlsl(
            model,
            Asserts.AsA<IStandardMaterial>(material),
            shaderRequirements),
        FinShaderType.HIDDEN => new HiddenShaderSourceGlsl(),
        FinShaderType.NULL => new NullShaderSourceGlsl(model,
          shaderRequirements),
    };
  }
}