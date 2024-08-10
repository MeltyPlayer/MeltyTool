using fin.io.bundles;
using fin.util.progress;

using jsystem.api;

using uni.platforms.gcn;
using uni.platforms.gcn.tools;

namespace uni.games.twilight_princess;

public class TwilightPrincessFileBundleGatherer : IAnnotatedFileBundleGatherer {
  public void GatherFileBundles(
      IFileBundleOrganizer organizer,
      IMutablePercentageProgress mutablePercentageProgress) {
    if (!new GcnFileHierarchyExtractor().TryToExtractFromGame(
            "twilight_princess",
            out var fileHierarchy)) {
      return;
    }

    var objectDirectory
        = fileHierarchy.Root.AssertGetExistingSubdir(@"res\Object");
    {
      var yaz0Dec = new Yaz0Dec();
      var didDecompress = false;
      foreach (var arcFile in objectDirectory.FilesWithExtension(".arc")) {
        didDecompress |= yaz0Dec.Run(arcFile, true);
      }

      if (didDecompress) {
        objectDirectory.Refresh();
      }

      var rarcDump = new RarcDump();
      var didDump = false;
      foreach (var rarcFile in objectDirectory.FilesWithExtension(".rarc")) {
        didDump |= rarcDump.Run(rarcFile,
                                true,
                                new HashSet<string>(["archive"]));
      }

      if (didDump) {
        objectDirectory.Refresh(true);
      }
    }

    foreach (var bmdFile in fileHierarchy.Root.GetFilesWithFileType(
                 ".bmd",
                 true)) {
      organizer.Add(new BmdModelFileBundle {
          GameName = "twilight_princess",
          BmdFile = bmdFile,
          FrameRate = 60
      }.Annotate(bmdFile)); 
    }
  }
}