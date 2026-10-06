using fin.model;
using fin.util.strings;

namespace fin.shaders.glsl;

public static partial class GlslUtil {
  public static void AppendSelectedColorMutation(
      IndentedStringBuilder src,
      IReadOnlyMaterial material) {
    src.AppendBlock(
        $"if ({GlslConstants.UNIFORM_IS_SELECTED_NAME})",
        () => {
          src.AppendLine($"{GlslConstants.UNIFORM_FRAG_COLOR_NAME}.rgb = min({GlslConstants.UNIFORM_FRAG_COLOR_NAME}.rgb + .3, 1);");
        });
  }
}