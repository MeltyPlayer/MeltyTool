using fin.io;

namespace fin.common;

public static class CommonFiles {
  public static IReadOnlySystemDirectory COMMON_DIRECTORY { get; } =
    DirectoryConstants.BASE_DIRECTORY.AssertGetExistingSubdir("common");

  public static IReadOnlySystemFile WINDOWS_SOUNDFONT_FILE { get; } =
    COMMON_DIRECTORY.AssertGetExistingFile("windows.sf2");
}