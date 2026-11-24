using fin.util.types;

namespace fin.ui.rendering.gl;

[IocCandiate]
public static class OpenGlFeatureService {
  public static bool SupportsBindlessTextures { get; set; }
  public static bool SupportsNamedBufferStorage { get; set; }
  public static bool SupportsSsbos { get; set; }
}