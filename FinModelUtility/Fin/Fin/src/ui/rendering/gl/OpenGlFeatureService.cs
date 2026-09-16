using fin.util.types;

namespace fin.ui.rendering.gl;

[IocCandiate]
public static class OpenGlFeatureService {
  public static bool SupportsSsbos
    => !OpenGlVersionService.Es &&
       OpenGlVersionService.MajorVersion >= 4 &&
       OpenGlVersionService.MinorVersion >= 3;
}