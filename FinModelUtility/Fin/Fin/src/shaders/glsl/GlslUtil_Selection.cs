using fin.util.strings;

namespace fin.shaders.glsl;

public static partial class GlslUtil {
  public const bool ENABLE_SELECTION_VIA_SHADER = true;

  public static void MaybeAppendSelectedHeader(
      IndentedStringBuilder src,
      bool isSelectable) {
    if (!ENABLE_SELECTION_VIA_SHADER || !isSelectable) {
      return;
    }

    if (ENABLE_SELECTION_VIA_SHADER && isSelectable) {
      src.Append(
          $$"""
            layout (std430, binding = {{GlslConstants.UBO_SELECTION_BINDING_INDEX}}) readonly buffer {{GlslConstants.UBO_SELECTION_NAME}} {
              int isPrimitiveSelected[];
            };

            """);
    }  }

  public static void MaybeAppendSelectedColorMutation(
      IndentedStringBuilder src,
      bool isSelectable) {
    if (!ENABLE_SELECTION_VIA_SHADER || !isSelectable) {
      return;
    } 
    
    src.AppendBlock(
        "if (isPrimitiveSelected[gl_PrimitiveID] == 1)",
        () => {
          src.AppendLine(
              $"{GlslConstants.UNIFORM_FRAG_COLOR_NAME}.rgb = min({GlslConstants.UNIFORM_FRAG_COLOR_NAME}.rgb + .3, 1);");
        });
  }
}