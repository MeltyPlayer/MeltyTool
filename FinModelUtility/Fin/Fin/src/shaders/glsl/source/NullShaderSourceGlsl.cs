using fin.model;
using fin.util.enumerables;
using fin.util.strings;

namespace fin.shaders.glsl.source;

public sealed class NullShaderSourceGlsl(
    IReadOnlyModel model,
    IModelRequirements modelRequirements,
    IShaderRequirements shaderRequirements,
    bool isSelectable)
    : IShaderSourceGlsl {
  public string VertexShaderSource { get; } =
    GlslUtil.GetVertexSrc(model, modelRequirements, shaderRequirements, isSelectable);

  public string FragmentShaderSource {
    get {
      var sb = new IndentedStringBuilder();
      sb.AppendLine(
          $"""
           #version {GlslConstants.FRAGMENT_SHADER_VERSION}
           {GlslConstants.FLOAT_PRECISION}

           out vec4 ${GlslConstants.UNIFORM_FRAG_COLOR_NAME};
           """);

      var hasColors = shaderRequirements.UsedColors.AnyTrue();
      if (hasColors) {
        sb.AppendLine(
            $"""

             in vec4 {GlslConstants.IN_VERTEX_COLOR_NAME}0;
             """);
      }

      sb.AppendLine();
      sb.AppendBlock(
          "void main()",
          () => this.AppendFragmentMain(sb));

      return sb.ToString();
    }
  }

  public void AppendFragmentMain(IndentedStringBuilder sb) {
    sb.AppendLine(
        $"${GlslConstants.UNIFORM_FRAG_COLOR_NAME} = {(shaderRequirements.UsedColors.AnyTrue() ? $"{GlslConstants.IN_VERTEX_COLOR_NAME}0" : "vec4(1)")};");
    GlslUtil.MaybeAppendSelectedColorMutation(sb, isSelectable);
  }
}