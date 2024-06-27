using fin.io;

namespace uni.platforms.wii.tools {
  public static class WiiToolsConstants {
    public static IReadOnlySystemDirectory WIT_DIRECTORY { get; } =
      DirectoryConstants.TOOLS_DIRECTORY.AssertGetExistingSubdir("wit");

    public static IReadOnlySystemFile WIT_EXE { get; } =
      WiiToolsConstants.WIT_DIRECTORY.AssertGetExistingFile("wit.exe");
  }
}