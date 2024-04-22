using fin.model;
using fin.util.asserts;

namespace fin.shaders.glsl {
  public static class GlslMaterialExtensions {
    public static IShaderSourceGlsl ToShaderSource(
        this IReadOnlyMaterial? material,
        IReadOnlyModel model,
        bool useBoneMatrices)
      => material.GetShaderType() switch {
        FinShaderType.FIXED_FUNCTION
            => new FixedFunctionShaderSourceGlsl(model,
                                                 Asserts.AsA<IFixedFunctionMaterial>(material),
                                                 useBoneMatrices),
        FinShaderType.TEXTURE => new TextureShaderSourceGlsl(model, Asserts.AsA<ITextureMaterial>(material), useBoneMatrices),
        FinShaderType.COLOR => new ColorShaderSourceGlsl(model, Asserts.AsA<IColorMaterial>(material), useBoneMatrices),
        FinShaderType.STANDARD => new StandardShaderSourceGlsl(
              model,
              Asserts.AsA<IStandardMaterial>(material),
              useBoneMatrices),
        FinShaderType.NULL => new NullShaderSourceGlsl(model, useBoneMatrices),
      };
  }
}