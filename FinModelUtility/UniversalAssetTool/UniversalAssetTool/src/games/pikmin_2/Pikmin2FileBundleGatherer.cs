using ast.api;

using fin.io;
using fin.io.bundles;

using games.pikmin2.api;

using jsystem.api;

using uni.platforms.gcn;

namespace uni.games.pikmin_2 {
  public class Pikmin2FileBundleGatherer : IAnnotatedFileBundleGatherer {
    public IEnumerable<IAnnotatedFileBundle> GatherFileBundles() {
      if (!new GcnFileHierarchyExtractor().TryToExtractFromGame(
              "pikmin_2",
              GcnFileHierarchyExtractor.Options.Standard()
                                       .PruneRarcDumpNames("arc", "data"),
              out var fileHierarchy)) {
        return Enumerable.Empty<IAnnotatedFileBundle>();
      }

      return
          this.ExtractPikminAndCaptainModels_(fileHierarchy)
              .Concat(
                  this.ExtractAllFromSeparateDirectories_(fileHierarchy))
              .Concat(this.ExtractAllFromMergedDirectories_(fileHierarchy))
              .Concat(this.ExtractAllLevelScenes_(fileHierarchy))
              .Concat(this.ExtractLeafBudFlower_(fileHierarchy))
              .Concat(this.ExtractAllTreasures_(fileHierarchy))
              .Concat(this.GatherMapUnits_(fileHierarchy))
              .Concat(this.ExtractAudio_(fileHierarchy));
    }

    /// <summary>
    ///   Gets from separate model/animation szs (e.g. Enemies)
    /// </summary>
    private IEnumerable<IAnnotatedFileBundle>
        ExtractAllFromSeparateDirectories_(
            IFileHierarchy fileHierarchy) {
      foreach (var subdir in fileHierarchy) {
        var modelSubdir =
            subdir.GetExistingSubdirs()
                  .SingleOrDefault(dir => dir.Name == "model");
        var animSubdir =
            subdir.GetExistingSubdirs()
                  .SingleOrDefault(dir => dir.Name == "anim");

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
    private IEnumerable<IAnnotatedFileBundle> ExtractAllFromMergedDirectories_(
        IFileHierarchy fileHierarchy) {
      foreach (var subdir in fileHierarchy) {
        var arcSubdir =
            subdir.GetExistingSubdirs()
                  .SingleOrDefault(dir => dir.Name == "arc");

        if (arcSubdir != null &&
            arcSubdir.FilesWithExtension(".bmd").Any()) {
          foreach (var bundle in this.ExtractModelsInDirectoryAutomatically_(
                       arcSubdir)) {
            yield return bundle;
          }
        }
      }
    }

    private IEnumerable<IAnnotatedFileBundle> ExtractAllLevelScenes_(
        IFileHierarchy fileHierarchy) {
      var userDir = fileHierarchy.Root.AssertGetExistingSubdir("user");
      var abeMapRootDir = userDir.AssertGetExistingSubdir("Abe/map");
      var kandoMapRootDir = userDir.AssertGetExistingSubdir("Kando/map");

      foreach (var abeMapDir in abeMapRootDir.GetExistingSubdirs()) {
        var mapName = abeMapDir.Name;
        if (mapName == "zukan") {
          continue;
        }

        var kandoMapDir = kandoMapRootDir.AssertGetExistingSubdir(mapName);

        var mapBmd = kandoMapDir.AssertGetExistingFile("arc/model.bmd");
        var routeTxt = abeMapDir.AssertGetExistingFile("route.txt");

        yield return new Pikmin2SceneFileBundle {
            LevelBmd = mapBmd, RouteTxt = routeTxt,
        }.Annotate(mapBmd);
      }
    }

    private IEnumerable<IAnnotatedFileBundle> ExtractPikminAndCaptainModels_(
        IFileHierarchy fileHierarchy) {
      var pikminAndCaptainBaseDirectory =
          fileHierarchy.Root.AssertGetExistingSubdir(
              @"user\Kando\piki\pikis_designer");

      var bcxFiles =
          pikminAndCaptainBaseDirectory.AssertGetExistingSubdir("motion")
                                       .GetExistingFiles();

      var captainSubdir =
          pikminAndCaptainBaseDirectory.AssertGetExistingSubdir("orima_model");
      var pikminSubdir =
          pikminAndCaptainBaseDirectory.AssertGetExistingSubdir("piki_model");

      return this.ExtractModels_(captainSubdir.GetExistingFiles(), bcxFiles)
                 .Concat(
                     this.ExtractModels_(pikminSubdir.GetExistingFiles(),
                                         bcxFiles));
    }

    private IEnumerable<IAnnotatedFileBundle> ExtractAllTreasures_(
        IFileHierarchy fileHierarchy) {
      var treasureBaseDirectory =
          fileHierarchy.Root.AssertGetExistingSubdir(@"user\Abe\Pellet");

      foreach (var locale in treasureBaseDirectory.GetExistingSubdirs()) {
        foreach (var treasure in locale.GetExistingSubdirs()) {
          var bmdFiles = treasure.GetFilesWithFileType(".bmd")
                                 .ToArray();
          if (bmdFiles.Length > 0) {
            var bcxFiles =
                treasure.GetExistingFiles()
                        .Where(file => file.FileType == ".bca" ||
                                       file.FileType == ".bck")
                        .ToList();
            foreach (var bundle in this.ExtractModels_(bmdFiles, bcxFiles)) {
              yield return bundle;
            }
          }
        }
      }
    }

    private IEnumerable<IAnnotatedFileBundle> GatherMapUnits_(
        IFileHierarchy fileHierarchy) {
      var mapUnitsSubdir =
          fileHierarchy.Root.AssertGetExistingSubdir(
              @"user\Mukki\mapunits\arc");

      foreach (var mapUnitSubdir in mapUnitsSubdir.GetExistingSubdirs()) {
        var bmdFiles = mapUnitSubdir.GetFilesWithFileType(".bmd", true)
                                    .ToArray();
        if (bmdFiles.Length > 0) {
          var btiFiles =
              mapUnitSubdir.GetFilesWithFileType(".bti", true)
                           .ToList();
          foreach (var bundle in
                   this.ExtractModels_(bmdFiles, null, btiFiles)) {
            yield return bundle;
          }
        }
      }
    }

    private IEnumerable<IAnnotatedFileBundle> ExtractAudio_(
        IFileHierarchy fileHierarchy)
      => fileHierarchy.Root.AssertGetExistingSubdir(@"AudioRes\Stream")
                      .FilesWithExtension(".ast")
                      .Select(
                          astFile => new AstAudioFileBundle {
                              GameName = "pikmin_2",
                              AstFile = astFile
                          }.Annotate(astFile));

    private IEnumerable<IAnnotatedFileBundle> ExtractLeafBudFlower_(
        IFileHierarchy fileHierarchy)
      => this.ExtractModelsInDirectoryAutomatically_(
          fileHierarchy.Root.AssertGetExistingSubdir(
              @"user\Kando\piki\pikis_designer\happa_model"));

    private IEnumerable<IAnnotatedFileBundle>
        ExtractModelsInDirectoryAutomatically_(
            IFileHierarchyDirectory directory)
      => this.ExtractModels_(
          directory.FilesWithExtension(".bmd"),
          directory.FilesWithExtensions(".bca", ".bck")
                   .ToList(),
          directory.FilesWithExtension(".bti").ToList());

    private IEnumerable<IAnnotatedFileBundle> ExtractModels_(
        IEnumerable<IFileHierarchyFile> bmdFiles,
        IReadOnlyList<IFileHierarchyFile>? bcxFiles = null,
        IReadOnlyList<IFileHierarchyFile>? btiFiles = null
    )
      => bmdFiles.Select(bmdFile => new BmdModelFileBundle {
          GameName = "pikmin_2",
          BmdFile = bmdFile,
          BcxFiles = bcxFiles,
          BtiFiles = btiFiles,
      }.Annotate(bmdFile));
  }
}