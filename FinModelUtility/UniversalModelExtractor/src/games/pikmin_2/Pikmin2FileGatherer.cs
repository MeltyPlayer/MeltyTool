using ast.api;

using j3d.exporter;

using fin.io;
using fin.io.bundles;

using uni.platforms;
using uni.platforms.gcn;


namespace uni.games.pikmin_2 {
  public class Pikmin2FileGatherer : IFileBundleGatherer<IFileBundle> {
    public IEnumerable<IFileBundle> GatherFileBundles(
        bool assert) {
      if (!DirectoryConstants.ROMS_DIRECTORY.PossiblyAssertExistingFile(
              "pikmin_2.gcm",
              assert,
              out var pikmin2Rom)) {
        return Enumerable.Empty<IFileBundle>();
      }

      var options = GcnFileHierarchyExtractor.Options.Standard()
                                             .PruneRarcDumpNames("arc", "data");
      var fileHierarchy =
          new GcnFileHierarchyExtractor().ExtractFromRom(
              options,
              pikmin2Rom);

      return
          this.ExtractPikminAndCaptainModels_(fileHierarchy)
              .Concat(
                  this.ExtractAllFromSeparateDirectories_(fileHierarchy))
              .Concat(this.ExtractAllFromMergedDirectories_(fileHierarchy))
              .Concat(this.ExtractLeafBudFlower_(fileHierarchy))
              .Concat(this.ExtractAllTreasures_(fileHierarchy))
              .Concat(this.ExtractAudio_(fileHierarchy));
    }

    /// <summary>
    ///   Gets from separate model/animation szs (e.g. Enemies)
    /// </summary>
    private IEnumerable<IFileBundle> ExtractAllFromSeparateDirectories_(
        IFileHierarchy fileHierarchy) {
      foreach (var subdir in fileHierarchy) {
        var modelSubdir =
            subdir.Subdirs.SingleOrDefault(dir => dir.Name == "model");
        var animSubdir =
            subdir.Subdirs.SingleOrDefault(dir => dir.Name == "anim");

        if (modelSubdir != null && animSubdir != null) {
          var bmdFiles = modelSubdir.FilesWithExtension(".bmd").ToArray();
          var bcxFiles =
              animSubdir.FilesWithExtensions(".bca", ".bck").ToArray();
          var btiFiles = subdir.FilesWithExtensionRecursive(".bti").ToArray();

          foreach (var bundle in this.ExtractModels_(bmdFiles,
                     bcxFiles,
                     btiFiles)) {
            yield return bundle;
          }
        }
      }
    }

    /// <summary>
    ///   Gets from model/animations in same szs (e.g. user\Kando)
    /// </summary>
    private IEnumerable<IFileBundle> ExtractAllFromMergedDirectories_(
        IFileHierarchy fileHierarchy) {
      foreach (var subdir in fileHierarchy) {
        var arcSubdir =
            subdir.Subdirs.SingleOrDefault(dir => dir.Name == "arc");

        if (arcSubdir != null &&
            arcSubdir.FilesWithExtension(".bmd").Any()) {
          foreach (var bundle in this.ExtractModelsInDirectoryAutomatically_(
                       arcSubdir)) {
            yield return bundle;
          }
        }
      }
    }

    private IEnumerable<IFileBundle> ExtractPikminAndCaptainModels_(
        IFileHierarchy fileHierarchy) {
      var pikminAndCaptainBaseDirectory =
          fileHierarchy.Root.GetExistingSubdir(
              @"user\Kando\piki\pikis_designer");

      var bcxFiles =
          pikminAndCaptainBaseDirectory.GetExistingSubdir("motion")
                                       .Files;

      var captainSubdir =
          pikminAndCaptainBaseDirectory.GetExistingSubdir("orima_model");
      var pikminSubdir =
          pikminAndCaptainBaseDirectory.GetExistingSubdir("piki_model");

      return this.ExtractModels_(captainSubdir.Files, bcxFiles)
                 .Concat(this.ExtractModels_(pikminSubdir.Files, bcxFiles));
    }

    private IEnumerable<IFileBundle> ExtractAllTreasures_(
        IFileHierarchy fileHierarchy) {
      var treasureBaseDirectory =
          fileHierarchy.Root.GetExistingSubdir(@"user\Abe\Pellet");

      foreach (var locale in treasureBaseDirectory.Subdirs) {
        foreach (var treasure in locale.Subdirs) {
          var bmdFiles = treasure.Files.Where(file => file.Extension == ".bmd")
                                 .ToArray();
          if (bmdFiles.Length > 0) {
            var bcxFiles =
                treasure.Files
                        .Where(file => file.Extension == ".bca" ||
                                       file.Extension == ".bck")
                        .ToList();
            foreach (var bundle in this.ExtractModels_(bmdFiles, bcxFiles)) {
              yield return bundle;
            }
          }
        }
      }
    }

    private IEnumerable<IFileBundle> ExtractAudio_(
        IFileHierarchy fileHierarchy)
      => fileHierarchy.Root.GetExistingSubdir(@"AudioRes\Stream")
                      .FilesWithExtension(".ast")
                      .Select(
                          astFile => new AstAudioFileBundle {
                                  GameName = "pikmin_2",
                                  AstFile = astFile
                              });

    private IEnumerable<IFileBundle> ExtractLeafBudFlower_(
        IFileHierarchy fileHierarchy)
      => this.ExtractModelsInDirectoryAutomatically_(
          fileHierarchy.Root.GetExistingSubdir(
              @"user\Kando\piki\pikis_designer\happa_model"));

    private IEnumerable<IFileBundle> ExtractModelsInDirectoryAutomatically_(
        IFileHierarchyDirectory directory)
      => this.ExtractModels_(
          directory.FilesWithExtension(".bmd"),
          directory.FilesWithExtensions(".bca", ".bck")
                   .ToList(),
          directory.FilesWithExtension(".bti").ToList());

    private IEnumerable<IFileBundle> ExtractModels_(
        IEnumerable<IFileHierarchyFile> bmdFiles,
        IReadOnlyList<IFileHierarchyFile>? bcxFiles = null,
        IReadOnlyList<IFileHierarchyFile>? btiFiles = null
    )
      => bmdFiles.Select(bmdFile => new BmdModelFileBundle {
          GameName = "pikmin_2",
          BmdFile = bmdFile,
          BcxFiles = bcxFiles,
          BtiFiles = btiFiles,
      });
  }
}