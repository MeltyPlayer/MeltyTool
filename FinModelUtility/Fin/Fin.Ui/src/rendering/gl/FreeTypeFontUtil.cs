using fin.common;

using QuickFont;

namespace fin.ui.rendering.gl;

public static class FreeTypeFontUtil {
  private static bool isInit_ = false;

  public static void InitIfNeeded() {
    if (isInit_) {
      return;
    }

    FreeTypeFont.Init(DirectoryConstants.DLL_DIRECTORY);
    isInit_ = true;
  }
}