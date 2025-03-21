using fin.io;
using fin.log;
using fin.util.asserts;
using fin.util.cmd;

using uni.platforms.gcn.tools;

namespace uni.platforms.threeDs.tools;

public class ThreeDsXfsaTool {
  public bool Extract(IFileHierarchyFile xsfaFile) {
      Asserts.True(xsfaFile.Exists,
                   $"Could not extract archive because it does not exist: {xsfaFile.FullPath}");

      var directoryPath = xsfaFile.FullNameWithoutExtension;
      var directory = new FinDirectory(directoryPath);

      if (directory.Exists) {
        return false;
      }

      var logger = Logging.Create<ThreeDsXfsaTool>();
      logger.LogInformation($"Extracting XSFA {xsfaFile.LocalPath}...");

      Files.RunInDirectory(
          xsfaFile.Parent!.Impl,
          () => {
            ProcessUtil.ExecuteBlockingSilently(
                ThreeDsToolsConstants.THREEDS_XSFATOOL_EXE,
                $"-i \"{xsfaFile.FullPath}\" " +
                $"-o \"{directory.FullPath}\" " + "-q");
          });

      Asserts.True(directory.Exists);

      return true;
    }
}