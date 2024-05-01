using System.IO;

using fin.io;
using fin.io.bundles;

using uni.platforms;

using UoT.api;
using UoT.memory;

namespace uni.games.ocarina_of_time {
  public class OcarinaOfTimeFileBundleGatherer
      : IAnnotatedFileBundleGatherer<OotModelFileBundle> {
    public IEnumerable<IAnnotatedFileBundle<OotModelFileBundle>>
        GatherFileBundles() {
      if (!DirectoryConstants.ROMS_DIRECTORY.TryToGetExistingFile(
              "ocarina_of_time.z64",
              out var ocarinaOfTimeRom)) {
        yield break;
      }

      var ocarinaOfTimeDirectory =
          ExtractorUtil.GetOrCreateExtractedDirectory("ocarina_of_time");
      var fileHierarchy = FileHierarchy.From("ocarina_of_time", ocarinaOfTimeDirectory);
      var root = fileHierarchy.Root;

      var rootSysDir = root.Impl;
      var zObjectsDir = rootSysDir.GetOrCreateSubdir("zObjects");

      var zSegments = ZSegments.InitializeFromFile(ocarinaOfTimeRom);
      var zObjectsAndPaths = zSegments.Objects.Select(zObject => {
        var path = Path.Join(zObjectsDir.Name, $"{zObject.FileName}.zobj");
        return (zObject, path);
      });

      foreach (var (_, path) in zObjectsAndPaths) {
        var zObjectFile = new FinFile(Path.Join(rootSysDir.FullPath, path));

        // TODO: Write the actual data here
        FinFileSystem.File.Create(zObjectFile.FullPath);
      }

      root.Refresh(true);
      foreach (var (zObject, path) in zObjectsAndPaths) {
        var zObjectFile = root.AssertGetExistingFile(path);
        yield return new OotModelFileBundle(
            root,
            ocarinaOfTimeRom,
            zObject).Annotate(zObjectFile);
      }
    }
  }
}