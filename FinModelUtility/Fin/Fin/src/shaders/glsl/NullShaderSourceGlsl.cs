using System.Text;

using fin.model;
using fin.util.enumerables;

namespace fin.shaders.glsl;

public class NullShaderSourceGlsl(
    IReadOnlyModel model,
    IShaderRequirements shaderRequirements)
    : IShaderSourceGlsl {
  public string VertexShaderSource { get; } =
    GlslUtil.GetVertexSrc(model, shaderRequirements);

  public string FragmentShaderSource {
    get {
      var sb = new StringBuilder();
      sb.AppendLine(
          $"""
           #version {GlslConstants.FRAGMENT_SHADER_VERSION}
           {GlslConstants.FLOAT_PRECISION}

           out vec4 fragColor;
           """);

      var hasColors = shaderRequirements.UsedColors.AnyTrue();
      if (hasColors) {
        sb.AppendLine(
            $"""

             in vec4 {GlslConstants.IN_VERTEX_COLOR_NAME}0;
             """);
      }

      sb.Append($$"""

                  void main() {
                    fragColor = {{(hasColors ? $"{GlslConstants.IN_VERTEX_COLOR_NAME}0" : "vec4(1)")}};
                  }
                  """);

      return sb.ToString();
    }
  }
}