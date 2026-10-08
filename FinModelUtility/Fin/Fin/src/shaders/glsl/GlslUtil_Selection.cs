using fin.util.strings;

namespace fin.shaders.glsl;

public static partial class GlslUtil {
  public const bool ENABLE_SELECTION_VIA_SHADER = false;

  public static void MaybeAppendSelectedHeader(
      IndentedStringBuilder src,
      bool isSelectable) {
    if (!ENABLE_SELECTION_VIA_SHADER || !isSelectable) {
      return;
    }


  }

  public static void MaybeAppendSelectedColorMutation(
      IndentedStringBuilder src,
      bool isSelectable) {
    if (!ENABLE_SELECTION_VIA_SHADER || !isSelectable) {
      return;
    } 
    
    src.AppendBlock(
        $"if ({GlslConstants.UNIFORM_IS_SELECTED_NAME})",
        () => {
          src.AppendLine(
              $"{GlslConstants.UNIFORM_FRAG_COLOR_NAME}.rgb = min({GlslConstants.UNIFORM_FRAG_COLOR_NAME}.rgb + .3, 1);");
        });
  }
}