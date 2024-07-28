using System.Collections.Immutable;

using fin.io;

using uni.platforms.gcn.tools;

namespace uni.platforms.threeDs.tools.ctrtool;

public static partial class Ctrtool {
  private static readonly object CTRTOOL_LOCK = new();

  private static readonly ImmutableHashSet<string> EXPECTED_FILE_NAMES =
      new[] { "ctrtool.exe", "extract_cci.bat", "extract_cia.bat", }
          .ToImmutableHashSet();

  private static void RunInCtrDirectoryAndCleanUp_(Action handler) {
    lock (CTRTOOL_LOCK) {
      Files.RunInDirectory(ThreeDsToolsConstants.CTRTOOL_DIRECTORY, handler);

      foreach (var fileToCleanUp in ThreeDsToolsConstants
                                    .CTRTOOL_DIRECTORY.GetExistingFiles()
                                    .Where(file => !EXPECTED_FILE_NAMES
                                               .Contains(file.Name))) {
        fileToCleanUp.Delete();
      }
    }
  }
}