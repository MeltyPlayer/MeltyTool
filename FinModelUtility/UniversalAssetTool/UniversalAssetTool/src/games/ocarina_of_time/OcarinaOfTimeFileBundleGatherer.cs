using f3dzex2.io;

using fin.io;
using fin.io.bundles;
using fin.util.progress;

using schema.util.streams;

using uni.platforms;

using UoT.api;
using UoT.memory;

namespace uni.games.ocarina_of_time;

public class OcarinaOfTimeFileBundleGatherer : IAnnotatedFileBundleGatherer {
  public void GatherFileBundles(
      IFileBundleOrganizer organizer,
      IMutablePercentageProgress mutablePercentageProgress) {
    if (!DirectoryConstants.ROMS_DIRECTORY.TryToGetExistingFile(
            "ocarina_of_time.z64",
            out var ocarinaOfTimeRom)) {
      return;
    }

    var ocarinaOfTimeDirectory =
        ExtractorUtil.GetOrCreateExtractedDirectory("ocarina_of_time");
    var fileHierarchy
        = FileHierarchy.From("ocarina_of_time", ocarinaOfTimeDirectory);
    var root = fileHierarchy.Root;

    var rootSysDir = root.Impl;
    var zObjectsDir = rootSysDir.GetOrCreateSubdir("zObjects");

    var zSegments = ZSegments.InitializeFromFile(ocarinaOfTimeRom);
    var zObjectsAndPaths = zSegments.Objects.Select(zObject => {
      var path = Path.Join(zObjectsDir.Name, $"{zObject.FileName}.zobj");
      return (zObject, path);
    });

    {
      var n64Memory = new N64Memory(ocarinaOfTimeRom);

      foreach (var (zObject, path) in zObjectsAndPaths) {
        var zObjectFile = new FinFile(Path.Join(rootSysDir.FullPath, path));
        if (!zObjectFile.Exists) {
          using var fw = zObjectFile.OpenWrite();
          using var br = n64Memory.OpenSegment(zObject.Segment);
          br.CopyTo(fw);
        }
      }
    }

    root.Refresh(true);
    foreach (var (zObject, path) in zObjectsAndPaths) {
      var zObjectFile = root.AssertGetExistingFile(path);
      organizer.Add(new OotModelFileBundle(
                        root,
                        ocarinaOfTimeRom,
                        zObject).Annotate(zObjectFile));
    }
  }
}