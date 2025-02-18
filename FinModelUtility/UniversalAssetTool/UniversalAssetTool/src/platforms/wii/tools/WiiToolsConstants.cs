using fin.common;
using fin.io;

namespace uni.platforms.wii.tools;

public static class WiiToolsConstants {
  public static IReadOnlySystemDirectory WIT_DIRECTORY { get; } =
    DirectoryConstants.TOOLS_DIRECTORY.AssertGetExistingSubdir("wit");

  public static IReadOnlySystemFile WIT_EXE { get; } =
    WIT_DIRECTORY.AssertGetExistingFile("wit.exe");
}