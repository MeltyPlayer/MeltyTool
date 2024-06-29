using ast.api;

using fin.io;
using fin.io.bundles;
using fin.util.asserts;
using fin.util.progress;

using jsystem.api;

using uni.platforms.gcn;

namespace uni.games.mario_kart_double_dash {
  public class MarioKartDoubleDashFileBundleGatherer
      : IAnnotatedFileBundleGatherer {
    public IEnumerable<IAnnotatedFileBundle> GatherFileBundles(
        IMutablePercentageProgress mutablePercentageProgress) {
      if (!new GcnFileHierarchyExtractor().TryToExtractFromGame(
              "mario_kart_double_dash",
              GcnFileHierarchyExtractor.Options.Standard()
                                       .UseRarcDumpForExtensions(".arc"),
              out var fileHierarchy)) {
        return Enumerable.Empty<IAnnotatedFileBundle>();
      }

      // TODO: Extract "enemies"
      // TODO: Extract "objects"
      return this.ExtractDrivers_(fileHierarchy)
                 .Concat(this.ExtractKarts_(fileHierarchy))
                 .Concat(this.ExtractCourses_(fileHierarchy))
                 .Concat(this.ExtractAudio_(fileHierarchy));
    }

    private IEnumerable<IAnnotatedFileBundle> ExtractKarts_(
        IFileHierarchy fileHierarchy)
      => fileHierarchy.Root.AssertGetExistingSubdir(@"MRAM\kart")
                      .GetExistingSubdirs()
                      .Select(subdir => subdir.FilesWithExtension(".bmd"))
                      .SelectMany(bmdFiles => this.ExtractModels_(bmdFiles));

    private IEnumerable<IAnnotatedFileBundle> ExtractDrivers_(
        IFileHierarchy fileHierarchy) {
      var mramSubdir =
          fileHierarchy.Root.AssertGetExistingSubdir(@"MRAM\driver");

      {
        var plumberNames = new[] { "mario", "luigi", };
        var plumberSubdirs =
            mramSubdir.GetExistingSubdirs()
                      .Where(
                          subdir => plumberNames.Contains(subdir.Name));
        var plumberCommon = mramSubdir.AssertGetExistingSubdir("cmn_hige");
        foreach (var plumberSubdir in plumberSubdirs) {
          foreach (var bundle in this.ExtractFromSeparateDriverDirectories_(
                       plumberSubdir,
                       plumberCommon)) {
            yield return bundle;
          }
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
                          subdir => babyNames.Contains(subdir.Name));
        var babyCommon = mramSubdir.AssertGetExistingSubdir("cmn_baby");
        foreach (var babySubdir in babySubdirs) {
          foreach (var bundle in this.ExtractFromSeparateDriverDirectories_(
                       babySubdir,
                       babyCommon)) {
            yield return bundle;
          }
        }
      }

      {
        var princessNames = new[] { "daisy", "peach" };
        var princessSubdirs =
            mramSubdir.GetExistingSubdirs()
                      .Where(
                          subdir => princessNames.Contains(subdir.Name));
        var princessCommon = mramSubdir.AssertGetExistingSubdir("cmn_hime");
        foreach (var princessSubdir in princessSubdirs) {
          foreach (var bundle in this.ExtractFromSeparateDriverDirectories_(
                       princessSubdir,
                       princessCommon)) {
            yield return bundle;
          }
        }
      }

      {
        var lizardNames = new[] { "catherine", "yoshi" };
        var lizardSubdirs =
            mramSubdir.GetExistingSubdirs()
                      .Where(
                          subdir => lizardNames.Contains(subdir.Name));
        var lizardCommon = mramSubdir.AssertGetExistingSubdir("cmn_liz");
        foreach (var lizardSubdir in lizardSubdirs) {
          foreach (var bundle in this.ExtractFromSeparateDriverDirectories_(
                       lizardSubdir,
                       lizardCommon)) {
            yield return bundle;
          }
        }
      }

      // TODO: Where are toad's animations?

      {
        var koopaNames = new[] { "patapata", "nokonoko" };
        var koopaSubdirs =
            mramSubdir.GetExistingSubdirs()
                      .Where(
                          subdir => koopaNames.Contains(subdir.Name));
        var koopaCommon = mramSubdir.AssertGetExistingSubdir("cmn_zako");
        foreach (var koopaSubdir in koopaSubdirs) {
          foreach (var bundle in this.ExtractFromSeparateDriverDirectories_(
                       koopaSubdir,
                       koopaCommon)) {
            yield return bundle;
          }
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
                          subdir => standaloneNames.Contains(subdir.Name));
        foreach (var standaloneSubdir in standaloneSubdirs) {
          foreach (var bundle in this.ExtractFromDriverDirectory_(
                       standaloneSubdir)) {
            yield return bundle;
          }
        }
      }
    }

    private IEnumerable<IAnnotatedFileBundle> ExtractFromDriverDirectory_(
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
      foreach (var bundle in this.ExtractModels_(driverBmdFiles,
                                                 driverBcxFiles)) {
        yield return bundle;
      }

      var otherBmdFiles = bmdFiles.Where(file => !driverBmdFiles.Contains(file))
                                  .ToArray();
      if (otherBmdFiles.Length > 0) {
        var otherBcxFiles =
            bcxFiles.Where(file => !driverBcxFiles.Contains(file))
                    .ToArray();
        foreach (var bundle in
                 this.ExtractModels_(otherBmdFiles, otherBcxFiles)) {
          yield return bundle;
        }
      }
    }

    private IEnumerable<IAnnotatedFileBundle>
        ExtractFromSeparateDriverDirectories_(
            IFileHierarchyDirectory directory,
            IFileHierarchyDirectory common) {
      Asserts.Nonnull(common);

      var bmdFiles = directory.FilesWithExtension(".bmd")
                              .ToArray();
      var commonBcxFiles = common.FilesWithExtensions(".bca", ".bck")
                                 .ToArray();
      var localBcxFiles = directory.FilesWithExtensions(".bca", ".bck")
                                   .ToArray();

      return this.ExtractModels_(
          bmdFiles,
          commonBcxFiles.Concat(localBcxFiles).ToArray());
    }

    private IEnumerable<IAnnotatedFileBundle> ExtractCourses_(
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

        foreach (var bundle in this.ExtractModels_(bmdFiles, null, btiFiles)) {
          yield return bundle;
        }

        var objectsSubdir = subdir.AssertGetExistingSubdir("objects");
        foreach (var bundle in this.ExtractModelsAndAnimationsFromSceneObject_(
                     objectsSubdir)) {
          yield return bundle;
        }
      }
    }

    private IEnumerable<IAnnotatedFileBundle> ExtractAudio_(
        IFileHierarchy fileHierarchy)
      => fileHierarchy.Root.AssertGetExistingSubdir(@"AudioRes\Stream")
                      .FilesWithExtension(".ast")
                      .Select(astFile => new AstAudioFileBundle {
                          GameName = "mario_kart_double_dash",
                          AstFile = astFile,
                      }.Annotate(astFile));

    private IEnumerable<IAnnotatedFileBundle>
        ExtractModelsAndAnimationsFromSceneObject_(
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
        return bmdFiles.SelectMany(bmdFile => this.ExtractModels_(
                                       new[] { bmdFile },
                                       allBcxFiles,
                                       btiFiles));
      }

      var unclaimedBcxFiles = allBcxFiles.ToHashSet();
      var bmdAndBcxFiles =
          new Dictionary<IFileHierarchyFile, IFileHierarchyFile[]>();
      foreach (var bmdFile in bmdFiles) {
        var prefix = bmdFile.Name;
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
      return bmdAndBcxFiles.SelectMany(pair => {
        var (bmdFile, bcxFiles) = pair;
        return this.ExtractModels_(new[] { bmdFile }, bcxFiles, btiFiles);
      });
    }

    private IEnumerable<IAnnotatedFileBundle> ExtractModels_(
        IEnumerable<IFileHierarchyFile> bmdFiles,
        IReadOnlyList<IFileHierarchyFile>? bcxFiles = null,
        IReadOnlyList<IFileHierarchyFile>? btiFiles = null
    )
      => bmdFiles.Select(bmdFile => new BmdModelFileBundle {
          GameName = "mario_kart_double_dash",
          BmdFile = bmdFile,
          BcxFiles = bcxFiles,
          BtiFiles = btiFiles,
          FrameRate = 60,
      }.Annotate(bmdFile));
  }
}