using ast.api;

using fin.io;
using fin.io.bundles;
using fin.util.progress;

using games.pikmin2.api;

using jsystem.api;

using uni.platforms.gcn;

namespace uni.games.pikmin_2;

public class Pikmin2FileBundleGatherer : BGameCubeFileBundleGatherer {
  public override string Name => "pikmin_2";

  public override GcnFileHierarchyExtractor.Options Options 
    => GcnFileHierarchyExtractor.Options.Standard()
                                .PruneRarcDumpNames("arc", "data");

  protected override void GatherFileBundlesFromHierarchy(
      IFileBundleOrganizer organizer,
      IMutablePercentageProgress mutablePercentageProgress,
      IFileHierarchy fileHierarchy) {
    new AnnotatedFileBundleGathererAccumulatorWithInput<IFileHierarchy>(
            fileHierarchy)
        .Add(this.ExtractPikminAndCaptainModels_)
        .Add(this.ExtractAllFromSeparateDirectories_)
        .Add(this.ExtractAllFromMergedDirectories_)
        .Add(this.ExtractAllLevelScenes_)
        .Add(this.ExtractLeafBudFlower_)
        .Add(this.ExtractAllTreasures_)
        .Add(this.GatherMapUnits_)
        .Add(this.ExtractAudio_)
        .GatherFileBundles(organizer, mutablePercentageProgress);
  }

  /// <summary>
  ///   Gets from separate model/animation szs (e.g. Enemies)
  /// </summary>
  private void ExtractAllFromSeparateDirectories_(
      IFileBundleOrganizer organizer,
      IFileHierarchy fileHierarchy) {
    foreach (var subdir in fileHierarchy) {
      var modelSubdir
          = subdir.GetExistingSubdirs().SingleOrDefaultByName("model");
      var animSubdir
          = subdir.GetExistingSubdirs().SingleOrDefaultByName("anim");

      if (modelSubdir != null && animSubdir != null) {
        var bmdFiles = modelSubdir.FilesWithExtension(".bmd").ToArray();
        var bcxFiles =
            animSubdir.FilesWithExtensions(".bca", ".bck").ToArray();
        var btiFiles = subdir.FilesWithExtensionRecursive(".bti").ToArray();

        this.ExtractModels_(organizer, bmdFiles, bcxFiles, btiFiles);
      }
    }
  }

  /// <summary>
  ///   Gets from model/animations in same szs (e.g. user\Kando)
  /// </summary>
  private void ExtractAllFromMergedDirectories_(
      IFileBundleOrganizer organizer,
      IFileHierarchy fileHierarchy) {
    foreach (var subdir in fileHierarchy) {
      var arcSubdir = subdir.GetExistingSubdirs().SingleOrDefaultByName("arc");
      if (arcSubdir != null &&
          arcSubdir.FilesWithExtension(".bmd").Any()) {
        this.ExtractModelsInDirectoryAutomatically_(organizer, arcSubdir);
      }
    }
  }

  private void ExtractAllLevelScenes_(IFileBundleOrganizer organizer,
                                      IFileHierarchy fileHierarchy) {
    var userDir = fileHierarchy.Root.AssertGetExistingSubdir("user");
    var abeMapRootDir = userDir.AssertGetExistingSubdir("Abe/map");
    var kandoMapRootDir = userDir.AssertGetExistingSubdir("Kando/map");

    foreach (var abeMapDir in abeMapRootDir.GetExistingSubdirs()) {
      var mapName = abeMapDir.Name;
      if (mapName.SequenceEqual("zukan")) {
        continue;
      }

      var kandoMapDir = kandoMapRootDir.AssertGetExistingSubdir(mapName);

      var mapBmd = kandoMapDir.AssertGetExistingFile("arc/model.bmd");
      var routeTxt = abeMapDir.AssertGetExistingFile("route.txt");

      organizer.Add(new Pikmin2SceneFileBundle {
          LevelBmd = mapBmd, RouteTxt = routeTxt,
      }.Annotate(mapBmd));
    }
  }

  private void ExtractPikminAndCaptainModels_(IFileBundleOrganizer organizer,
                                              IFileHierarchy fileHierarchy) {
    var pikminAndCaptainBaseDirectory =
        fileHierarchy.Root.AssertGetExistingSubdir(
            @"user\Kando\piki\pikis_designer");

    var bcxFiles =
        pikminAndCaptainBaseDirectory.AssertGetExistingSubdir("motion")
                                     .GetExistingFiles()
                                     .ToArray();

    var captainSubdir =
        pikminAndCaptainBaseDirectory.AssertGetExistingSubdir("orima_model");
    var pikminSubdir =
        pikminAndCaptainBaseDirectory.AssertGetExistingSubdir("piki_model");

    this.ExtractModels_(organizer,
                        captainSubdir.GetExistingFiles(),
                        bcxFiles);
    this.ExtractModels_(organizer,
                        pikminSubdir.GetExistingFiles(),
                        bcxFiles);
  }

  private void ExtractAllTreasures_(
      IFileBundleOrganizer organizer,
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
          this.ExtractModels_(organizer, bmdFiles, bcxFiles);
        }
      }
    }
  }

  private void GatherMapUnits_(IFileBundleOrganizer organizer,
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
        this.ExtractModels_(organizer, bmdFiles, null, btiFiles);
      }
    }
  }

  private void ExtractAudio_(IFileBundleOrganizer organizer,
                             IFileHierarchy fileHierarchy) {
    var astFiles = fileHierarchy
                   .Root.AssertGetExistingSubdir(@"AudioRes\Stream")
                   .FilesWithExtension(".ast");
    foreach (var astFile in astFiles) {
      organizer.Add(new AstAudioFileBundle {
          AstFile = astFile
      }.Annotate(astFile));
    }
  }

  private void ExtractLeafBudFlower_(IFileBundleOrganizer organizer,
                                     IFileHierarchy fileHierarchy)
    => this.ExtractModelsInDirectoryAutomatically_(
        organizer,
        fileHierarchy.Root.AssertGetExistingSubdir(
            @"user\Kando\piki\pikis_designer\happa_model"));

  private void ExtractModelsInDirectoryAutomatically_(
      IFileBundleOrganizer organizer,
      IFileHierarchyDirectory directory)
    => this.ExtractModels_(
        organizer,
        directory.FilesWithExtension(".bmd"),
        directory.FilesWithExtensions(".bca", ".bck")
                 .ToList(),
        directory.FilesWithExtension(".bti").ToList());

  private void ExtractModels_(
      IFileBundleOrganizer organizer,
      IEnumerable<IFileHierarchyFile> bmdFiles,
      IReadOnlyList<IFileHierarchyFile>? bcxFiles = null,
      IReadOnlyList<IFileHierarchyFile>? btiFiles = null
  ) {
    foreach (var bmdFile in bmdFiles) {
      organizer.Add(new BmdModelFileBundle {
          BmdFile = bmdFile,
          BcxFiles = bcxFiles,
          BtiFiles = btiFiles,
      }.Annotate(bmdFile));
    }
  }
}