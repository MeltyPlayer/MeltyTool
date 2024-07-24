using fin.io;
using fin.log;
using fin.util.asserts;

using uni.util.cmd;

namespace uni.platforms.gcn.tools;

public class Yaz0Dec {
  public bool Run(IFileHierarchyFile file, bool cleanup) {
      Asserts.True(
          file.Exists,
          $"Cannot decrypt YAZ0 because it does not exist: {file}");

      var finalRarcPath = file.FullNameWithoutExtension + ".rarc";
      if (File.Exists(finalRarcPath)) {
        return false;
      }

      if (!MagicTextUtil.Verify(file, "Yaz0")) {
        return false;
      }

      var rarcPath = file.FullPath + " 0.rarc";
      if (!File.Exists(rarcPath)) {
        var logger = Logging.Create<Yaz0Dec>();
        Files.RunInDirectory(
            file.Impl.AssertGetParent()!,
            () => {
              ProcessUtil.ExecuteBlockingSilently(
                  GcnToolsConstants.YAZ0DEC_EXE,
                  $"\"{file.FullPath}\"");
            });
        Asserts.True(File.Exists(rarcPath),
                     $"File was not created: {rarcPath}");
      }

      File.Move(rarcPath, finalRarcPath);
      if (cleanup) {
        file.Impl.Delete();
      }

      return true;
    }
}