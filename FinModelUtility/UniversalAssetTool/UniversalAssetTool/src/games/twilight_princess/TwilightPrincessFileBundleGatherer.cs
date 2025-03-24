using fin.io.bundles;
using fin.util.progress;

using gx.archives.rarc;
using gx.compression.yaz0;

using jsystem.api;

using uni.platforms.gcn;


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
        fileHierarchy.RefreshRootAndUpdateCache();
      }
    }

    foreach (var dir in objectDirectory.GetExistingSubdirs()) {
      if (dir.TryToGetExistingSubdir("bmdr", out var bmdrDirectory)) {
        var bmdFiles = bmdrDirectory.GetExistingFiles().ToArray();
        if (bmdFiles.Length == 1) {
          var animations = dir.FilesWithExtensionsRecursive([".bca", ".bck"]);
          organizer.Add(new BmdModelFileBundle {
              BmdFile = bmdFiles[0],
              BcxFiles = animations.ToArray(),
              FrameRate = 30
          }.Annotate(bmdFiles[0]));
        }
      } else {
        foreach (var bmdFile in dir.GetFilesWithFileType(".bmd", true)) {
          organizer.Add(new BmdModelFileBundle {
              BmdFile = bmdFile,
          }.Annotate(bmdFile));
        }
      }
    }
  }
}