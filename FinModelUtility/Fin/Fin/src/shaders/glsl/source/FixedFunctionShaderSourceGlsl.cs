using fin.language.equations.fixedFunction;
using fin.model;

namespace fin.shaders.glsl.source;

public class FixedFunctionShaderSourceGlsl(
    IReadOnlyModel model,
    IModelRequirements modelRequirements,
    IFixedFunctionMaterial material,
    IShaderRequirements shaderRequirements)
    : IShaderSourceGlsl {
  public string VertexShaderSource { get; }
    = GlslUtil.GetVertexSrc(model, modelRequirements, shaderRequirements);

  public string FragmentShaderSource { get; } =
    new FixedFunctionEquationsGlslPrinter(model).Print(
        material,
        shaderRequirements);
}