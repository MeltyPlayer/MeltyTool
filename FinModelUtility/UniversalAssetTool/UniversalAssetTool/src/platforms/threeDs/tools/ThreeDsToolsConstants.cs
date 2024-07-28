using fin.io;

namespace uni.platforms.gcn.tools;

public static class ThreeDsToolsConstants {
  public static ISystemDirectory CTRTOOL_DIRECTORY { get; } =
    DirectoryConstants.TOOLS_DIRECTORY.AssertGetExistingSubdir("ctrtool");

  public static IReadOnlySystemFile EXTRACT_CIA_BAT { get; } =
    CTRTOOL_DIRECTORY.AssertGetExistingFile(
        "extract_cia.bat");

  public static IReadOnlySystemFile EXTRACT_CCI_BAT { get; } =
    CTRTOOL_DIRECTORY.AssertGetExistingFile(
        "extract_cci.bat");


  public static IReadOnlySystemDirectory THREEDS_XSFATOOL_DIRECTORY { get; } =
    DirectoryConstants.TOOLS_DIRECTORY.AssertGetExistingSubdir(
        "3ds-xfsatool");

  public static IReadOnlySystemFile THREEDS_XSFATOOL_EXE { get; } =
    THREEDS_XSFATOOL_DIRECTORY.AssertGetExistingFile(
        "3ds-xfsatool.exe");
}