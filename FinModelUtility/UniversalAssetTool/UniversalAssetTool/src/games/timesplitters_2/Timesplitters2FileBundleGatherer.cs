using fin.io;
using fin.io.archive;
using fin.io.bundles;
using fin.util.progress;

namespace uni.games.timesplitters_2;

public class Timesplitters2FileBundleGatherer : BGameCubeFileBundleGatherer {
  public override string Name => "timesplitters_2";

  protected override void GatherFileBundlesFromHierarchy(
      IFileBundleOrganizer organizer,
      IMutablePercentageProgress mutablePercentageProgress,
      IFileHierarchy fileHierarchy) {
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