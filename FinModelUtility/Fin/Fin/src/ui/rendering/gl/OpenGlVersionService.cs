using fin.util.types;

namespace fin.ui.rendering.gl;

[IocCandiate]
public static class OpenGlVersionService {
  public static void Init(bool isOpenGlEs) {
    Es = isOpenGlEs;

    if (isOpenGlEs) {
      Init(false, 3, 1);
    } else {
      Init(false, 4, 6);
    }
  } 

  public static void Init(bool isOpenGlEs, int majorVersion, int minorVersion) {
    Es = isOpenGlEs;
    MajorVersion = majorVersion;
    MinorVersion = minorVersion;
  }

  public static bool Es { get; private set; }
  public static int MajorVersion { get; private set; }
  public static int MinorVersion { get; private set; }
}