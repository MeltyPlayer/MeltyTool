namespace fin.shaders.glsl.source;

public sealed class HiddenShaderSourceGlsl : IShaderSourceGlsl {
  public string VertexShaderSource

    => $$"""
         {{GlslConstants.FRAGMENT_HEADER_WITH_TEXTURES}}

         void main() {}
         """;

  public string FragmentShaderSource
    => $$"""
         {{GlslConstants.FRAGMENT_HEADER_WITHOUT_TEXTURES}}
         
         void main() {
           discard;
         }
         """;
}