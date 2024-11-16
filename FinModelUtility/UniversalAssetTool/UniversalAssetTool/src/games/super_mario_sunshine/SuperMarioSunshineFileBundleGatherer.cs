using fin.io;
using fin.io.bundles;
using fin.util.asserts;
using fin.util.progress;

using jsystem.api;

using uni.platforms.gcn;

namespace uni.games.super_mario_sunshine;

public class SuperMarioSunshineFileBundleGatherer
    : IAnnotatedFileBundleGatherer {
  public void GatherFileBundles(
      IFileBundleOrganizer organizer,
      IMutablePercentageProgress mutablePercentageProgress) {
    if (!new GcnFileHierarchyExtractor().TryToExtractFromGame(
            "super_mario_sunshine",
            GcnFileHierarchyExtractor.Options.Standard()
                                     .PruneRarcDumpNames("scene"),
            out var fileHierarchy)) {
      return;
    }

    new AnnotatedFileBundleGathererAccumulatorWithInput<IFileHierarchy>(
            fileHierarchy)
        .Add(this.ExtractMario_)
        .Add(this.ExtractFludd_)
        .Add(this.ExtractYoshi_)
        .Add(this.ExtractScenes_)
        .GatherFileBundles(organizer, mutablePercentageProgress);
  }

  private void ExtractMario_(IFileBundleOrganizer organizer,
                             IFileHierarchy fileHierarchy) {
    var marioSubdir =
        fileHierarchy.Root.AssertGetExistingSubdir(@"data\mario");
    var bcxFiles = marioSubdir.AssertGetExistingSubdir("bck")
                              .GetExistingFiles()
                              .Where(
                                  file => file.Name.StartsWith("ma_"))
                              .ToArray();
    var bmdFile = marioSubdir.AssertGetExistingSubdir("bmd")
                             .GetExistingFiles()
                             .SingleByName("ma_mdl1.bmd");

    this.ExtractModel_(organizer, bmdFile, bcxFiles);
  }

  private void ExtractFludd_(IFileBundleOrganizer organizer,
                             IFileHierarchy fileHierarchy) {
    var fluddSubdir =
        fileHierarchy.Root.AssertGetExistingSubdir(@"data\mario\watergun2");
    foreach (var subdir in fluddSubdir.GetExistingSubdirs()) {
      this.ExtractPrimaryAndSecondaryModels_(
          organizer,
          subdir,
          file => file.Name.IndexOf("wg") != -1);
    }
  }

  private void ExtractYoshi_(IFileBundleOrganizer organizer,
                             IFileHierarchy fileHierarchy) {
    var yoshiSubdir =
        fileHierarchy.Root.AssertGetExistingSubdir(@"data\yoshi");
    var bcxFiles = yoshiSubdir
                   .GetExistingFiles()
                   .Where(
                       file => file.FileType == ".bck")
                   // TODO: Look into this, this animation seems to need extra bone(s)?
                   .Where(file => !file.Name.StartsWith("yoshi_tongue"))
                   .ToArray();
    var bmdFile = yoshiSubdir.GetExistingFiles()
                             .SingleByName("yoshi_model.bmd");

    this.ExtractModel_(organizer, bmdFile, bcxFiles);
  }

  private void ExtractScenes_(
      IFileBundleOrganizer organizer,
      IFileHierarchy fileHierarchy) {
    var sceneSubdir =
        fileHierarchy.Root.AssertGetExistingSubdir(@"data\scene");

    foreach (var subdir in sceneSubdir.GetExistingSubdirs()) {
      var mapSubdir = subdir.AssertGetExistingSubdir("map");
      var bmdFiles = mapSubdir.AssertGetExistingSubdir("map")
                              .GetExistingFiles()
                              .Where(file => file.FileType == ".bmd")
                              .ToArray();
      this.ExtractModels_(organizer, bmdFiles);

      var montemcommon =
          subdir.GetExistingSubdirs()
                .SingleOrDefaultByName("montemcommon");
      var montewcommon =
          subdir.GetExistingSubdirs()
                .SingleOrDefaultByName("montewcommon");
      var hamukurianm =
          subdir.GetExistingSubdirs()
                .SingleOrDefaultByName("hamukurianm");

      foreach (var objectSubdir in subdir.GetExistingSubdirs()) {
        var objName = objectSubdir.Name;

        IEnumerable<IAnnotatedFileBundle<BmdModelFileBundle>>? bundles = null;
        if (objName.StartsWith("montem") && objName.IndexOf("common") == -1) {
          this.ExtractFromSeparateDirectories_(
              organizer,
              objectSubdir,
              Asserts.CastNonnull(montemcommon));
        } else if (objName.StartsWith("montew") &&
                   objName.IndexOf("common") == -1) {
          this.ExtractFromSeparateDirectories_(
              organizer,
              objectSubdir,
              Asserts.CastNonnull(montewcommon));
        } else if (objName.StartsWith("hamukuri")) {
          if (objName.IndexOf("anm") == -1) {
            this.ExtractFromSeparateDirectories_(
                organizer,
                objectSubdir,
                Asserts.CastNonnull(hamukurianm));
          }
        } else {
          this.ExtractModelsAndAnimationsFromSceneObject_(
              organizer,
              objectSubdir);
        }
      }
    }
  }

  private void ExtractFromSeparateDirectories_(
      IFileBundleOrganizer organizer,
      IFileHierarchyDirectory directory,
      IFileHierarchyDirectory common) {
    Asserts.Nonnull(common);

    var bmdFiles = directory.FilesWithExtension(".bmd")
                            .ToArray();
    var commonBcxFiles = common.FilesWithExtensions(".bca", ".bck")
                               .ToArray();
    var commonBtiFiles = common.FilesWithExtension(".bti")
                               .ToArray();

    var localBcxFiles = directory.FilesWithExtensions(".bca", ".bck")
                                 .ToArray();
    if (bmdFiles.Length == 1) {
      this.ExtractModels_(
          organizer,
          bmdFiles,
          commonBcxFiles.Concat(localBcxFiles).ToArray(),
          commonBtiFiles);
      return;
    }

    try {
      Asserts.True(localBcxFiles.Length == 0);
      this.ExtractPrimaryAndSecondaryModels_(
          organizer,
          bmdFile => bmdFile.Name.StartsWith("default"),
          bmdFiles,
          commonBcxFiles);
    } catch {
      ;
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

    var specialCase = false;
    if (allBcxFiles.Length == 1 &&
        allBcxFiles[0].Name is "fish_swim.bck" &&
        bmdFiles.All(file => file.Name.StartsWith("fish"))) {
      specialCase = true;
    }

    if (allBcxFiles.Length == 1 &&
        allBcxFiles[0].Name is "butterfly_fly.bck" &&
        bmdFiles.All(file => file.Name.StartsWith("butterfly"))) {
      specialCase = true;
    }

    if (allBcxFiles.All(file => file.Name.StartsWith("popo_")) &&
        bmdFiles.All(file => file.Name.StartsWith("popo"))) {
      specialCase = true;
    }

    // If there is only one model or 0 animations, it's easy to tell which
    // animations go with which model.
    if (bmdFiles.Length == 1 || allBcxFiles.Length == 0 || specialCase) {
      this.ExtractModels_(organizer, bmdFiles, allBcxFiles);
      return;
    }

    if (directory.Name is "montemcommon" or "montewcommon") {
      this.ExtractModels_(organizer, bmdFiles);
      return;
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

        var bodyIndex = prefix.IndexOf("_body");
        if (bodyIndex != -1) {
          prefix = prefix.Substring(0, bodyIndex);
        }

        prefix = prefix.Replace("peach_hair_ponytail", "peach_hair_pony");
        prefix = prefix.Replace("eggyoshi_normal", "eggyoshi");
      }

      var claimedBcxFiles = unclaimedBcxFiles
                            .Where(bcxFile => bcxFile.Name.StartsWith(prefix))
                            .ToArray();

      foreach (var claimedBcxFile in claimedBcxFiles) {
        unclaimedBcxFiles.Remove(claimedBcxFile);
      }

      bmdAndBcxFiles[bmdFile] = claimedBcxFiles;
    }

    //Asserts.True(unclaimedBcxFiles.Count == 0);
    foreach (var (bmdFile, bcxFiles) in bmdAndBcxFiles) {
      this.ExtractModel_(organizer, bmdFile, bcxFiles);
    }
  }

  private void ExtractPrimaryAndSecondaryModels_(
      IFileBundleOrganizer organizer,
      IFileHierarchyDirectory directory,
      Func<IFileHierarchyFile, bool> primaryIdentifier
  ) {
    var bmdFiles = directory.GetExistingFiles()
                            .Where(
                                file => file.FileType == ".bmd")
                            .ToArray();
    var bcxFiles = directory
                   .GetExistingFiles()
                   .Where(
                       file => file.FileType == ".bck" ||
                               file.FileType == ".bca")
                   .ToArray();

    this.ExtractPrimaryAndSecondaryModels_(organizer,
                                           primaryIdentifier,
                                           bmdFiles,
                                           bcxFiles);
  }

  private void ExtractPrimaryAndSecondaryModels_(
      IFileBundleOrganizer organizer,
      Func<IFileHierarchyFile, bool> primaryIdentifier,
      IReadOnlyList<IFileHierarchyFile> bmdFiles,
      IReadOnlyList<IFileHierarchyFile>? bcxFiles = null
  ) {
    var primaryBmdFile =
        bmdFiles.Single(bmdFile => primaryIdentifier(bmdFile));
    this.ExtractModel_(organizer, primaryBmdFile, bcxFiles);

    var secondaryBmdFiles = bmdFiles
                            .Where(bmdFile => !primaryIdentifier(bmdFile))
                            .ToArray();
    if (secondaryBmdFiles.Length > 0) {
      this.ExtractModels_(organizer, secondaryBmdFiles);
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

  private void ExtractModel_(
      IFileBundleOrganizer organizer,
      IFileHierarchyFile bmdFile,
      IReadOnlyList<IFileHierarchyFile>? bcxFiles = null,
      IReadOnlyList<IFileHierarchyFile>? btiFiles = null
  ) => organizer.Add(new BmdModelFileBundle {
      GameName = "super_mario_sunshine",
      BmdFile = bmdFile,
      BcxFiles = bcxFiles,
      BtiFiles = btiFiles,
      FrameRate = 60
  }.Annotate(bmdFile));
}