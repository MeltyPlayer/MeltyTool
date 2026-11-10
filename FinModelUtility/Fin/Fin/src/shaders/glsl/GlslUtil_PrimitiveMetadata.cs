using fin.util.strings;

namespace fin.shaders.glsl;

public static partial class GlslUtil {
  public static void MaybeAppendPrimitiveMetadataHeader(
      IndentedStringBuilder src,
      bool isSelectable) {
    if (!isSelectable) {
      return;
    }

    if (isSelectable) {
      src.Append(
          $$"""
            layout (std430, binding = {{GlslConstants.UBO_PRIMITIVE_METADATA_BINDING_INDEX}}) readonly buffer {{GlslConstants.UBO_PRIMITIVE_METADATA_NAME}} {
              int isPrimitiveSelected[];
            };
            
            """);
    }  }

  public static void MaybeAppendSelectedColorMutation(
      IndentedStringBuilder src,
      bool isSelectable) {
    if (!isSelectable) {
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