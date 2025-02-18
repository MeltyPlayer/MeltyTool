using fin.common;
using fin.io;

namespace uni.platforms.gcn.tools;

public static class GcnToolsConstants {
  public static IReadOnlySystemDirectory SZSTOOLS_DIRECTORY { get; } =
    DirectoryConstants.TOOLS_DIRECTORY.AssertGetExistingSubdir("szstools");

  public static IReadOnlySystemFile RARCDUMP_EXE { get; } =
    SZSTOOLS_DIRECTORY
                     .AssertGetExistingFile("rarcdump.exe");

  public static IReadOnlySystemFile YAZ0DEC_EXE { get; } =
    SZSTOOLS_DIRECTORY.AssertGetExistingFile("yaz0dec.exe");


  public static IReadOnlySystemDirectory YAY0DEC_DIRECTORY { get; } =
    DirectoryConstants.TOOLS_DIRECTORY.AssertGetExistingSubdir("yay0dec");

  public static IReadOnlySystemFile YAY0DEC_EXE { get; } =
    YAY0DEC_DIRECTORY.AssertGetExistingFile("yay0dec.exe");
}