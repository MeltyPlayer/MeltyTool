using fin.io;
using fin.log;
using fin.util.asserts;
using fin.util.cmd;

using gx.tools;

namespace gx.archives.rarc;

public class RarcDump {
  public bool Run(
      IFileHierarchyFile rarcFile,
      bool cleanup,
      IReadOnlySet<string> junkTerms) {
    Asserts.True(
        rarcFile.Impl.Exists,
        $"Cannot dump RARC because it does not exist: {rarcFile}");

    if (!MagicTextUtil.Verify(rarcFile, "RARC")) {
      return false;
    }

    var directoryPath = rarcFile.FullPath + "_dir";
    if (!Directory.Exists(directoryPath)) {
      var logger = Logging.Create<RarcDump>();
      logger.LogInformation($"Dumping RARC {rarcFile.LocalPath}...");

      Files.RunInDirectory(
          rarcFile.Impl.AssertGetParent()!,
          () => {
            ProcessUtil.ExecuteBlockingSilently(
                GcnToolsConstants.RARCDUMP_EXE,
                $"\"{rarcFile.FullPath}\"");
          });
      Asserts.True(Directory.Exists(directoryPath),
                   $"Directory was not created: {directoryPath}");
    }

    var baseDirectoryName = Path.GetDirectoryName(directoryPath);

    // Determines final directory path.
    var directory = new FinDirectory(directoryPath);

    var existingSubdirs = directory.GetExistingSubdirs().ToArray();
    foreach (var subdir in existingSubdirs) {
      var subdirName = subdir.Name.ToString();

      string finalDirectoryName;
      if (existingSubdirs.Length > 1) {
        finalDirectoryName = subdirName;
      } else {
        var isSubdirJunk = junkTerms.Contains(subdirName);

        var rarcName = rarcFile.NameWithoutExtension.ToString();
        var isRarcJunk = junkTerms.Contains(rarcName);

        // If only one is in the junk set, uses the other.
        if (isSubdirJunk && !isRarcJunk) {
          finalDirectoryName = rarcName;
        } else if (!isSubdirJunk && isRarcJunk) {
          finalDirectoryName = subdirName;
        } 
        // If subdir has same name or is an abbreviation of the parent, 
        // just collapses them with the parent name.
        else if ((subdirName.Length <= rarcName.Length &&
                  subdirName.ToLower() ==
                  rarcName[..subdirName.Length].ToLower()) ||
                 (junkTerms?.Contains(subdirName) ?? false)) {
          finalDirectoryName = rarcName;
        }
        // If parent has same name or is an abbreviation of the subdir,
        // just collapses them with the subdir name.
        else if (subdirName.Length >= rarcName.Length &&
                 subdirName[..rarcName.Length].ToLower() ==
                 rarcName.ToLower()) {
          finalDirectoryName = subdirName;
        }
        // If subdir has a different name, merges their names together and
        // collapses them.
        else {
          finalDirectoryName = $"{rarcName}_{subdirName}";
        }
      }

      var finalDirectoryPath = Path.Join(baseDirectoryName, finalDirectoryName);

      Asserts.True(!Directory.Exists(finalDirectoryPath));
      subdir.MoveTo(finalDirectoryPath);
    }

    Directory.Delete(directoryPath);

    if (cleanup) {
      rarcFile.Impl.Delete();
    }

    return true;
  }
}