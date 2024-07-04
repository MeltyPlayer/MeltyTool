using fin.model;

namespace fin.shaders.glsl;

public class HiddenShaderSourceGlsl : IShaderSourceGlsl {
  public string VertexShaderSource

    => """
       #version 400

       void main() {}
       """;

  public string FragmentShaderSource
    => """
       #version 400

       void main() {
         discard;
       }
       """;
}