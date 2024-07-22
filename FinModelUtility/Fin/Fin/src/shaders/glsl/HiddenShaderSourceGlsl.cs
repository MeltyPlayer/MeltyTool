using fin.model;

namespace fin.shaders.glsl;

public class HiddenShaderSourceGlsl : IShaderSourceGlsl {
  public string VertexShaderSource

    => $$"""
         #version {{GlslConstants.SHADER_VERSION}}

         void main() {}
         """;

  public string FragmentShaderSource
    => $$"""
         #version {{GlslConstants.SHADER_VERSION}}

         void main() {
           discard;
         }
         """;
}