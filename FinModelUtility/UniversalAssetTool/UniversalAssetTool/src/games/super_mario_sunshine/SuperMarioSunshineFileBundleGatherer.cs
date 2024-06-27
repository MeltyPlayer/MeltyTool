using fin.io;
using fin.io.bundles;
using fin.util.asserts;

using jsystem.api;

using uni.platforms.gcn;

namespace uni.games.super_mario_sunshine;

public class SuperMarioSunshineFileBundleGatherer
    : IAnnotatedFileBundleGatherer<BmdModelFileBundle> {
  public IEnumerable<IAnnotatedFileBundle<BmdModelFileBundle>>
      GatherFileBundles() {
      if (!new GcnFileHierarchyExtractor().TryToExtractFromGame(
              "super_mario_sunshine",
              GcnFileHierarchyExtractor.Options.Standard()
                                       .PruneRarcDumpNames("scene"),
              out var fileHierarchy)) {
        return Enumerable.Empty<IAnnotatedFileBundle<BmdModelFileBundle>>();
      }

      return this.ExtractMario_(fileHierarchy)
                 .Concat(this.ExtractFludd_(fileHierarchy))
                 .Concat(this.ExtractYoshi_(fileHierarchy))
                 .Concat(this.ExtractScenes_(fileHierarchy));
    }

  private IEnumerable<IAnnotatedFileBundle<BmdModelFileBundle>> ExtractMario_(
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
                               .Single(
                                   file => file.Name == "ma_mdl1.bmd");

      return this.ExtractModels_(new[] { bmdFile }, bcxFiles);
    }

  private IEnumerable<IAnnotatedFileBundle<BmdModelFileBundle>> ExtractFludd_(
      IFileHierarchy fileHierarchy) {
      var fluddSubdir =
          fileHierarchy.Root.AssertGetExistingSubdir(@"data\mario\watergun2");
      foreach (var subdir in fluddSubdir.GetExistingSubdirs()) {
        foreach (var bundle in this.ExtractPrimaryAndSecondaryModels_(
                     subdir,
                     file => file.Name.Contains("wg"))) {
          yield return bundle;
        }
      }
    }

  private IEnumerable<IAnnotatedFileBundle<BmdModelFileBundle>> ExtractYoshi_(
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
                               .Single(
                                   file => file.Name == "yoshi_model.bmd");

      return this.ExtractModels_(new[] { bmdFile }, bcxFiles);
    }

  private IEnumerable<IAnnotatedFileBundle<BmdModelFileBundle>>
      ExtractScenes_(
          IFileHierarchy fileHierarchy) {
      var sceneSubdir =
          fileHierarchy.Root.AssertGetExistingSubdir(@"data\scene");

      foreach (var subdir in sceneSubdir.GetExistingSubdirs()) {
        var mapSubdir = subdir.AssertGetExistingSubdir("map");
        var bmdFiles = mapSubdir.AssertGetExistingSubdir("map")
                                .GetExistingFiles()
                                .Where(file => file.FileType == ".bmd")
                                .ToArray();
        foreach (var bundle in this.ExtractModels_(bmdFiles)) {
          yield return bundle;
        }

        var montemcommon =
            subdir.GetExistingSubdirs()
                  .SingleOrDefault(
                      subdir => subdir.Name == "montemcommon");
        var montewcommon =
            subdir.GetExistingSubdirs()
                  .SingleOrDefault(
                      subdir => subdir.Name == "montewcommon");
        var hamukurianm =
            subdir.GetExistingSubdirs()
                  .SingleOrDefault(
                      subdir => subdir.Name == "hamukurianm");

        foreach (var objectSubdir in subdir.GetExistingSubdirs()) {
          var objName = objectSubdir.Name;

          IEnumerable<IAnnotatedFileBundle<BmdModelFileBundle>>? bundles = null;
          if (objName.StartsWith("montem") && !objName.Contains("common")) {
            bundles = this.ExtractFromSeparateDirectories_(
                objectSubdir,
                Asserts.CastNonnull(montemcommon));
          } else if (objName.StartsWith("montew") &&
                     !objName.Contains("common")) {
            bundles = this.ExtractFromSeparateDirectories_(
                objectSubdir,
                Asserts.CastNonnull(montewcommon));
          } else if (objName.StartsWith("hamukuri")) {
            if (!objName.Contains("anm")) {
              bundles = this.ExtractFromSeparateDirectories_(
                  objectSubdir,
                  Asserts.CastNonnull(hamukurianm));
            }
          } else {
            bundles =
                this.ExtractModelsAndAnimationsFromSceneObject_(objectSubdir);
          }

          if (bundles != null) {
            foreach (var bundle in bundles) {
              yield return bundle;
            }
          }
        }
      }
    }

  private IEnumerable<IAnnotatedFileBundle<BmdModelFileBundle>>
      ExtractFromSeparateDirectories_(
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
        return this.ExtractModels_(bmdFiles,
                                   commonBcxFiles.Concat(localBcxFiles)
                                                 .ToArray(),
                                   commonBtiFiles);
      }

      try {
        Asserts.True(localBcxFiles.Length == 0);
        return this.ExtractPrimaryAndSecondaryModels_(
            bmdFile => bmdFile.Name.StartsWith(
                "default"),
            bmdFiles,
            commonBcxFiles);
      } catch {
        ;
      }

      return Enumerable.Empty<IAnnotatedFileBundle<BmdModelFileBundle>>();
    }

  private IEnumerable<IAnnotatedFileBundle<BmdModelFileBundle>>
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

      var specialCase = false;
      if (allBcxFiles.Length == 1 &&
          allBcxFiles[0].Name == "fish_swim.bck" &&
          bmdFiles.All(file => file.Name.StartsWith("fish"))) {
        specialCase = true;
      }

      if (allBcxFiles.Length == 1 &&
          allBcxFiles[0].Name == "butterfly_fly.bck" &&
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
        return this.ExtractModels_(bmdFiles, allBcxFiles);
      }

      if (directory.Name == "montemcommon" ||
          directory.Name == "montewcommon") {
        return this.ExtractModels_(bmdFiles);
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
      return bmdAndBcxFiles.SelectMany(pair => {
        var (bmdFile, bcxFiles) = pair;
        return this.ExtractModels_(new[] { bmdFile }, bcxFiles);
      });
    }

  private IEnumerable<IAnnotatedFileBundle<BmdModelFileBundle>>
      ExtractPrimaryAndSecondaryModels_(
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

      return this.ExtractPrimaryAndSecondaryModels_(primaryIdentifier,
                                                    bmdFiles,
                                                    bcxFiles);
    }

  private IEnumerable<IAnnotatedFileBundle<BmdModelFileBundle>>
      ExtractPrimaryAndSecondaryModels_(
          Func<IFileHierarchyFile, bool> primaryIdentifier,
          IReadOnlyList<IFileHierarchyFile> bmdFiles,
          IReadOnlyList<IFileHierarchyFile>? bcxFiles = null
      ) {
      var primaryBmdFile =
          bmdFiles.Single(bmdFile => primaryIdentifier(bmdFile));
      foreach (var bundle in this.ExtractModels_(
                   new[] { primaryBmdFile },
                   bcxFiles)) {
        yield return bundle;
      }

      var secondaryBmdFiles = bmdFiles
                              .Where(bmdFile => !primaryIdentifier(bmdFile))
                              .ToArray();
      if (secondaryBmdFiles.Length > 0) {
        foreach (var bundle in this.ExtractModels_(secondaryBmdFiles)) {
          yield return bundle;
        }
      }
    }

  private IEnumerable<IAnnotatedFileBundle<BmdModelFileBundle>>
      ExtractModels_(
          IEnumerable<IFileHierarchyFile> bmdFiles,
          IReadOnlyList<IFileHierarchyFile>? bcxFiles = null,
          IReadOnlyList<IFileHierarchyFile>? btiFiles = null
      )
    => bmdFiles.Select(bmdFile => new BmdModelFileBundle {
        GameName = "super_mario_sunshine",
        BmdFile = bmdFile,
        BcxFiles = bcxFiles,
        BtiFiles = btiFiles,
        FrameRate = 60
    }.Annotate(bmdFile));
}