using fin.io.archive;
using fin.io.bundles;
using fin.model.io;

using ts2;

using uni.platforms.gcn;

namespace uni.games.timesplitters_2;

public class Timesplitters2FileBundleGatherer
    : IAnnotatedFileBundleGatherer<IModelFileBundle> {
  public string Name => "timesplitters_2";

  public IEnumerable<IAnnotatedFileBundle<IModelFileBundle>>?
      GatherFileBundles() {
      if (!new GcnFileHierarchyExtractor().TryToExtractFromGame(
              "timesplitters_2",
              out var fileHierarchy)) {
        yield break;
      }

      var extractor = new SubArchiveExtractor();
      var pakFiles = fileHierarchy.Root.GetFilesWithFileType(".pak", true)
                                  .ToArray();
      if (pakFiles.Length > 0) {
        foreach (var pakFile in pakFiles) {
         /*extractor.ExtractRelativeToRoot<P8ckArchiveReader>(
              akFile,
              ileHierarchy.Root);*//
          //pakFile.Impl.Delete();
        }
      }
    }
}