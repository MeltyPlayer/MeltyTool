using fin.model;

namespace fin.shaders.glsl;

public class NullShaderSourceGlsl(IReadOnlyModel model, bool useBoneMatrices)
    : IShaderSourceGlsl {
  public string VertexShaderSource { get; } =
    GlslUtil.GetVertexSrc(model, useBoneMatrices);

  public string FragmentShaderSource
    => $$"""
       #version {{GlslConstants.SHADER_VERSION}}

       out vec4 fragColor;

       in vec4 vertexColor0;

       void main() {
         fragColor = vertexColor0;
       }
       """;
}