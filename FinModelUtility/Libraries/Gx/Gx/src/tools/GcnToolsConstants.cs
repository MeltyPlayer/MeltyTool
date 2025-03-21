using fin.common;
using fin.io;

namespace gx.tools;

public static class GcnToolsConstants {
  public static IReadOnlySystemDirectory SZSTOOLS_DIRECTORY { get; } =
    DirectoryConstants.TOOLS_DIRECTORY.AssertGetExistingSubdir("szstools");

  public static IReadOnlySystemFile RARCDUMP_EXE { get; } =
    SZSTOOLS_DIRECTORY.AssertGetExistingFile("rarcdump.exe");
}