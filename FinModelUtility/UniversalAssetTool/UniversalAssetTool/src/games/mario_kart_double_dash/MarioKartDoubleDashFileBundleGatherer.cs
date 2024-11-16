using ast.api;

using fin.io;
using fin.io.bundles;
using fin.util.asserts;
using fin.util.progress;

using jsystem.api;

using uni.platforms.gcn;

namespace uni.games.mario_kart_double_dash;

public class MarioKartDoubleDashFileBundleGatherer
    : IAnnotatedFileBundleGatherer {
  public void GatherFileBundles(
      IFileBundleOrganizer organizer,
      IMutablePercentageProgress mutablePercentageProgress) {
    if (!new GcnFileHierarchyExtractor().TryToExtractFromGame(
            "mario_kart_double_dash",
            GcnFileHierarchyExtractor.Options.Standard()
                                     .UseRarcDumpForExtensions(".arc"),
            out var fileHierarchy)) {
      return;
    }

    // TODO: Extract "enemies"
    // TODO: Extract "objects"
    new AnnotatedFileBundleGathererAccumulatorWithInput<IFileHierarchy>(
            fileHierarchy)
        .Add(this.ExtractDrivers_)
        .Add(this.ExtractKarts_)
        .Add(this.ExtractCourses_)
        .Add(this.ExtractAudio_)
        .GatherFileBundles(organizer, mutablePercentageProgress);
  }

  private void ExtractKarts_(IFileBundleOrganizer organizer,
                             IFileHierarchy fileHierarchy)
    => this.ExtractModels_(
        organizer,
        fileHierarchy.Root.AssertGetExistingSubdir(@"MRAM\kart")
                     .GetExistingSubdirs()
                     .SelectMany(
                         subdir => subdir.FilesWithExtension(".bmd")));

  private void ExtractDrivers_(IFileBundleOrganizer organizer,
                               IFileHierarchy fileHierarchy) {
    var mramSubdir =
        fileHierarchy.Root.AssertGetExistingSubdir(@"MRAM\driver");

    {
      var plumberNames = new[] { "mario", "luigi", };
      var plumberSubdirs =
          mramSubdir.GetExistingSubdirs()
                    .Where(
                        subdir => plumberNames.Contains(subdir.Name.ToString()));
      var plumberCommon = mramSubdir.AssertGetExistingSubdir("cmn_hige");
      foreach (var plumberSubdir in plumberSubdirs) {
        this.ExtractFromSeparateDriverDirectories_(organizer,
                                                   plumberSubdir,
                                                   plumberCommon);
      }
    }

    {
      var babyNames = new[] {
          "babymario",
          "babyluigi",
          // Should the toads actually be included here?
          "kinipio",
          "kinopico"
      };
      var babySubdirs =
          mramSubdir.GetExistingSubdirs()
                    .Where(
                        subdir => babyNames.Contains(subdir.Name.ToString()));
      var babyCommon = mramSubdir.AssertGetExistingSubdir("cmn_baby");
      foreach (var babySubdir in babySubdirs) {
        this.ExtractFromSeparateDriverDirectories_(organizer,
                                                   babySubdir,
                                                   babyCommon);
      }
    }

    {
      var princessNames = new[] { "daisy", "peach" };
      var princessSubdirs =
          mramSubdir.GetExistingSubdirs()
                    .Where(
                        subdir => princessNames.Contains(subdir.Name.ToString()));
      var princessCommon = mramSubdir.AssertGetExistingSubdir("cmn_hime");
      foreach (var princessSubdir in princessSubdirs) {
        this.ExtractFromSeparateDriverDirectories_(organizer,
                                                   princessSubdir,
                                                   princessCommon);
      }
    }

    {
      var lizardNames = new[] { "catherine", "yoshi" };
      var lizardSubdirs =
          mramSubdir.GetExistingSubdirs()
                    .Where(
                        subdir => lizardNames.Contains(subdir.Name.ToString()));
      var lizardCommon = mramSubdir.AssertGetExistingSubdir("cmn_liz");
      foreach (var lizardSubdir in lizardSubdirs) {
        this.ExtractFromSeparateDriverDirectories_(organizer,
                                                   lizardSubdir,
                                                   lizardCommon);
      }
    }

    // TODO: Where are toad's animations?

    {
      var koopaNames = new[] { "patapata", "nokonoko" };
      var koopaSubdirs =
          mramSubdir.GetExistingSubdirs()
                    .Where(
                        subdir => koopaNames.Contains(subdir.Name.ToString()));
      var koopaCommon = mramSubdir.AssertGetExistingSubdir("cmn_zako");
      foreach (var koopaSubdir in koopaSubdirs) {
        this.ExtractFromSeparateDriverDirectories_(
            organizer,
            koopaSubdir,
            koopaCommon);
      }
    }

    {
      var standaloneNames = new[] {
          "bosspakkun",
          "dk",
          "dkjr",
          "kingteresa",
          "koopa",
          "koopajr",
          "waluigi",
          "wario",
      };
      var standaloneSubdirs =
          mramSubdir.GetExistingSubdirs()
                    .Where(
                        subdir => standaloneNames.Contains(subdir.Name.ToString()));
      foreach (var standaloneSubdir in standaloneSubdirs) {
        this.ExtractFromDriverDirectory_(organizer, standaloneSubdir);
      }
    }
  }

  private void ExtractFromDriverDirectory_(IFileBundleOrganizer organizer,
                                           IFileHierarchyDirectory directory) {
    var bmdFiles = directory.FilesWithExtension(".bmd")
                            .ToArray();
    var bcxFiles = directory.FilesWithExtensions(".bca", ".bck")
                            .ToArray();

    var driverBmdFiles = bmdFiles
                         .Where(file => file.Name.StartsWith("driver"))
                         .ToArray();
    var driverBcxFiles =
        bcxFiles.Where(file => file.Name.StartsWith("b_") ||
                               file.Name.StartsWith("c_") ||
                               file.Name.StartsWith("all"))
                .ToArray();
    this.ExtractModels_(organizer, driverBmdFiles, driverBcxFiles);

    var otherBmdFiles = bmdFiles.Where(file => !driverBmdFiles.Contains(file))
                                .ToArray();
    if (otherBmdFiles.Length > 0) {
      var otherBcxFiles =
          bcxFiles.Where(file => !driverBcxFiles.Contains(file))
                  .ToArray();
      this.ExtractModels_(organizer, otherBmdFiles, otherBcxFiles);
    }
  }

  private void ExtractFromSeparateDriverDirectories_(
      IFileBundleOrganizer organizer,
      IFileHierarchyDirectory directory,
      IFileHierarchyDirectory common) {
    Asserts.Nonnull(common);

    var commonBcxFiles = common.FilesWithExtensions(".bca", ".bck")
                               .ToArray();
    var localBcxFiles = directory.FilesWithExtensions(".bca", ".bck")
                                 .ToArray();

    this.ExtractModels_(
        organizer,
        directory.FilesWithExtension(".bmd"),
        commonBcxFiles.Concat(localBcxFiles).ToArray());
  }

  private void ExtractCourses_(IFileBundleOrganizer organizer,
                               IFileHierarchy fileHierarchy) {
    var courseSubdir = fileHierarchy.Root.AssertGetExistingSubdir("Course");
    foreach (var subdir in courseSubdir.GetExistingSubdirs()) {
      var bmdFiles = subdir.FilesWithExtension(".bmd")
                           .ToArray();
      if (bmdFiles.Length == 0) {
        continue;
      }

      var btiFiles = subdir.FilesWithExtension(".bti")
                           .ToArray();

      this.ExtractModels_(organizer, bmdFiles, null, btiFiles);

      var objectsSubdir = subdir.AssertGetExistingSubdir("objects");
      this.ExtractModelsAndAnimationsFromSceneObject_(
          organizer,
          objectsSubdir);
    }
  }

  private void ExtractAudio_(IFileBundleOrganizer organizer,
                             IFileHierarchy fileHierarchy) {
    var astFiles = fileHierarchy
                   .Root.AssertGetExistingSubdir(@"AudioRes\Stream")
                   .FilesWithExtension(".ast");
    foreach (var astFile in astFiles) {
      organizer.Add(new AstAudioFileBundle {
          GameName = "mario_kart_double_dash",
          AstFile = astFile,
      }.Annotate(astFile));
    }
  }

  private void ExtractModelsAndAnimationsFromSceneObject_(
      IFileBundleOrganizer organizer,
      IFileHierarchyDirectory directory) {
    var bmdFiles = directory.GetExistingFiles()
                            .Where(
                                file => file.FileType == ".bmd")
                            .OrderByDescending(file => file.Name.Length)
                            .ToArray();
    var allBcxFiles = directory
                      .GetExistingFiles()
                      .Where(
                          file => file.FileType == ".bck" ||
                                  file.FileType == ".bca")
                      .ToArray();
    var btiFiles = directory.FilesWithExtension(".bti")
                            .ToArray();

    // If there is only one model or 0 animations, it's easy to tell which
    // animations go with which model.
    if (bmdFiles.Length == 1 || allBcxFiles.Length == 0) {
      foreach (var bmdFile in bmdFiles) {
        this.ExtractModel_(organizer, bmdFile, allBcxFiles, btiFiles);
      }
    }

    var unclaimedBcxFiles = allBcxFiles.ToHashSet();
    var bmdAndBcxFiles =
        new Dictionary<IFileHierarchyFile, IFileHierarchyFile[]>();
    foreach (var bmdFile in bmdFiles) {
      var prefix = bmdFile.Name.ToString();
      prefix = prefix.Substring(0, prefix.Length - ".bmd".Length);

      // Blegh. These special cases are gross.
      {
        var modelIndex = prefix.IndexOf("_model");
        if (modelIndex != -1) {
          prefix = prefix.Substring(0, modelIndex);
        }

        var babyIndex = prefix.IndexOf("_body");
        if (babyIndex != -1) {
          prefix = prefix.Substring(0, babyIndex);
        }

        // TODO: Fix animations shared by piantas
      }

      var claimedBcxFiles = unclaimedBcxFiles
                            .Where(bcxFile => bcxFile.Name.StartsWith(prefix))
                            .ToArray();

      foreach (var claimedBcxFile in claimedBcxFiles) {
        unclaimedBcxFiles.Remove(claimedBcxFile);
      }

      bmdAndBcxFiles[bmdFile] = claimedBcxFiles;
    }

    Asserts.True(unclaimedBcxFiles.Count == 0);
    foreach (var (bmdFile, bcxFiles) in bmdAndBcxFiles) {
      this.ExtractModel_(organizer, bmdFile, bcxFiles, btiFiles);
    }
  }

  private void ExtractModels_(
      IFileBundleOrganizer organizer,
      IEnumerable<IFileHierarchyFile> bmdFiles,
      IReadOnlyList<IFileHierarchyFile>? bcxFiles = null,
      IReadOnlyList<IFileHierarchyFile>? btiFiles = null
  ) {
    foreach (var bmdFile in bmdFiles) {
      this.ExtractModel_(organizer, bmdFile, bcxFiles, btiFiles);
    }
  }

  private void ExtractModel_(IFileBundleOrganizer organizer,
                             IFileHierarchyFile bmdFile,
                             IReadOnlyList<IFileHierarchyFile>? bcxFiles = null,
                             IReadOnlyList<IFileHierarchyFile>? btiFiles = null)
    => organizer.Add(new BmdModelFileBundle {
        GameName = "mario_kart_double_dash",
        BmdFile = bmdFile,
        BcxFiles = bcxFiles,
        BtiFiles = btiFiles,
        FrameRate = 60,
    }.Annotate(bmdFile));
}