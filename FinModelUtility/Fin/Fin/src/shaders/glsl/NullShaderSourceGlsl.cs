using fin.model;

namespace fin.shaders.glsl;

public class NullShaderSourceGlsl(
    IReadOnlyModel model,
    bool useBoneMatrices,
    IShaderRequirements shaderRequirements)
    : IShaderSourceGlsl {
  public string VertexShaderSource { get; } =
    GlslUtil.GetVertexSrc(model, useBoneMatrices, shaderRequirements);

  public string FragmentShaderSource
    => $$"""
         #version {{GlslConstants.SHADER_VERSION}}

         out vec4 fragColor;

         in vec4 {{GlslConstants.IN_VERTEX_COLOR_NAME}}0;

         void main() {
           fragColor = {{GlslConstants.IN_VERTEX_COLOR_NAME}}0;
         }
         """;
}