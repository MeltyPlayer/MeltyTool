using fin.language.equations.fixedFunction;
using fin.model;

namespace fin.shaders.glsl.source;

public sealed class FixedFunctionShaderSourceGlsl(
    IReadOnlyModel model,
    IModelRequirements modelRequirements,
    IReadOnlyFixedFunctionMaterial material,
    IShaderRequirements shaderRequirements,
    bool isSelectable)
    : IShaderSourceGlsl {
  public string VertexShaderSource { get; }
    = GlslUtil.GetVertexSrc(model, modelRequirements, shaderRequirements);

  public string FragmentShaderSource { get; } =
    new FixedFunctionEquationsGlslPrinter(model, isSelectable).Print(
        material,
        shaderRequirements);
}