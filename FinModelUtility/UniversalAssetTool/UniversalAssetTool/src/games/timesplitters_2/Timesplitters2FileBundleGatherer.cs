using fin.io.archive;
using fin.io.bundles;
using fin.util.progress;

using uni.platforms.gcn;

namespace uni.games.timesplitters_2 {
  public class Timesplitters2FileBundleGatherer : IAnnotatedFileBundleGatherer {
    public string Name => "timesplitters_2";

    public void GatherFileBundles(
        IFileBundleOrganizer organizer,
        IMutablePercentageProgress mutablePercentageProgress) {
      if (!new GcnFileHierarchyExtractor().TryToExtractFromGame(
              "timesplitters_2",
              out var fileHierarchy)) {
        return;
      }

      var extractor = new SubArchiveExtractor();
      var pakFiles = fileHierarchy.Root.GetFilesWithFileType(".pak", true)
                                  .ToArray();
      if (pakFiles.Length > 0) {
        foreach (var pakFile in pakFiles) {
          /*extractor.ExtractRelativeToRoot<P8ckArchiveReader>(
              pakFile,
              fileHierarchy.Root);*/
          //pakFile.Impl.Delete();
        }
      }
    }
  }
}