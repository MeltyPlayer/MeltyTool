using System;

using fin.util.asserts;

namespace fin.io;

public static class Files {
  public static ISystemDirectory GetCwd()
    => new FinDirectory(FinFileSystem.Directory.GetCurrentDirectory());

  private static readonly object RUN_IN_DIRECTORY_LOCK_ = new();

  public static void RunInDirectory(ISystemDirectory directory, Action handler) {
    lock (RUN_IN_DIRECTORY_LOCK_) {
      var cwd = FinFileSystem.Directory.GetCurrentDirectory();

      Asserts.True(directory.Exists,
                   $"Attempted to run in nonexistent directory: {directory}");
      FinFileSystem.Directory.SetCurrentDirectory(directory.FullPath);

      try {
        handler();
      } catch {
        FinFileSystem.Directory.SetCurrentDirectory(cwd);
        throw;
      }

      FinFileSystem.Directory.SetCurrentDirectory(cwd);
    }
  }


  // Getting Files
  public static string AssertValidExtension(string extension) {
    Asserts.True(extension.StartsWith("."));
    return extension;
  }
}