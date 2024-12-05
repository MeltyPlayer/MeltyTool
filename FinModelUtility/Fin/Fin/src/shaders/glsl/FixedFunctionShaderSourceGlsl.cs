using fin.language.equations.fixedFunction;
using fin.model;

namespace fin.shaders.glsl;

public class FixedFunctionShaderSourceGlsl(
    IReadOnlyModel model,
    IFixedFunctionMaterial material,
    IShaderRequirements shaderRequirements)
    : IShaderSourceGlsl {
  public string VertexShaderSource { get; }
    = GlslUtil.GetVertexSrc(model, shaderRequirements);

  public string FragmentShaderSource { get; } =
    new FixedFunctionEquationsGlslPrinter(model).Print(
        material,
        shaderRequirements);
}