namespace fin.shaders.glsl;

public class HiddenShaderSourceGlsl : IShaderSourceGlsl {
  public string VertexShaderSource

    => $$"""
         #version {{GlslConstants.FRAGMENT_SHADER_VERSION}}

         void main() {}
         """;

  public string FragmentShaderSource
    => $$"""
         #version {{GlslConstants.FRAGMENT_SHADER_VERSION}}
         {{GlslConstants.FLOAT_PRECISION}}
         
         void main() {
           discard;
         }
         """;
}