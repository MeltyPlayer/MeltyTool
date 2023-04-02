using j3d.exporter;

using fin.io;
using fin.io.bundles;
using fin.util.asserts;

using uni.platforms;
using uni.platforms.gcn;


namespace uni.games.super_mario_sunshine {
  public class SuperMarioSunshineModelFileGatherer
      : IFileBundleGatherer<BmdModelFileBundle> {
    public IEnumerable<BmdModelFileBundle> GatherFileBundles(bool assert) {
      if (!DirectoryConstants.ROMS_DIRECTORY.PossiblyAssertExistingFile(
              "super_mario_sunshine.gcm",
              assert,
              out var superMarioSunshineRom)) {
        return Enumerable.Empty<BmdModelFileBundle>();
      }

      var options = GcnFileHierarchyExtractor.Options.Standard()
                                             .PruneRarcDumpNames("scene");
      var fileHierarchy =
          new GcnFileHierarchyExtractor()
              .ExtractFromRom(options, superMarioSunshineRom);

      return this.ExtractMario_(fileHierarchy)
                 .Concat(this.ExtractFludd_(fileHierarchy))
                 .Concat(this.ExtractYoshi_(fileHierarchy))
                 .Concat(this.ExtractScenes_(fileHierarchy));
    }

    private IEnumerable<BmdModelFileBundle> ExtractMario_(
        IFileHierarchy fileHierarchy) {
      var marioSubdir =
          fileHierarchy.Root.GetExistingSubdir(@"data\mario");
      var bcxFiles = marioSubdir.GetExistingSubdir("bck")
                                .Files.Where(
                                    file => file.Name.StartsWith("ma_"))
                                .ToArray();
      var bmdFile = marioSubdir.GetExistingSubdir("bmd")
                               .Files.Single(
                                   file => file.Name == "ma_mdl1.bmd");

      return this.ExtractModels_(new[] { bmdFile }, bcxFiles);
    }

    private IEnumerable<BmdModelFileBundle> ExtractFludd_(
        IFileHierarchy fileHierarchy) {
      var fluddSubdir =
          fileHierarchy.Root.GetExistingSubdir(@"data\mario\watergun2");
      foreach (var subdir in fluddSubdir.Subdirs) {
        foreach (var bundle in this.ExtractPrimaryAndSecondaryModels_(
                     subdir,
                     file => file.Name.Contains("wg"))) {
          yield return bundle;
        }
      }
    }

    private IEnumerable<BmdModelFileBundle> ExtractYoshi_(
        IFileHierarchy fileHierarchy) {
      var yoshiSubdir =
          fileHierarchy.Root.GetExistingSubdir(@"data\yoshi");
      var bcxFiles = yoshiSubdir
                     .Files.Where(
                         file => file.Extension == ".bck")
                     // TODO: Look into this, this animation seems to need extra bone(s)?
                     .Where(file => !file.Name.StartsWith("yoshi_tongue"))
                     .ToArray();
      var bmdFile = yoshiSubdir.Files.Single(
          file => file.Name == "yoshi_model.bmd");

      return this.ExtractModels_(new[] { bmdFile }, bcxFiles);
    }

    private IEnumerable<BmdModelFileBundle> ExtractScenes_(
        IFileHierarchy fileHierarchy) {
      var sceneSubdir =
          fileHierarchy.Root.GetExistingSubdir(@"data\scene");

      foreach (var subdir in sceneSubdir.Subdirs) {
        var mapSubdir = subdir.GetExistingSubdir("map");
        var bmdFiles = mapSubdir.GetExistingSubdir("map")
                                .Files.Where(file => file.Extension == ".bmd")
                                .ToArray();
        foreach (var bundle in this.ExtractModels_(bmdFiles)) {
          yield return bundle;
        }

        var montemcommon =
            subdir.Subdirs.SingleOrDefault(
                subdir => subdir.Name == "montemcommon");
        var montewcommon =
            subdir.Subdirs.SingleOrDefault(
                subdir => subdir.Name == "montewcommon");
        var hamukurianm =
            subdir.Subdirs.SingleOrDefault(
                subdir => subdir.Name == "hamukurianm");

        foreach (var objectSubdir in subdir.Subdirs) {
          var objName = objectSubdir.Name;

          IEnumerable<BmdModelFileBundle>? bundles = null;
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

    private IEnumerable<BmdModelFileBundle> ExtractFromSeparateDirectories_(
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

      return Enumerable.Empty<BmdModelFileBundle>();
    }

    private IEnumerable<BmdModelFileBundle>
        ExtractModelsAndAnimationsFromSceneObject_(
            IFileHierarchyDirectory directory) {
      var bmdFiles = directory.Files.Where(
                                  file => file.Extension == ".bmd")
                              .OrderByDescending(file => file.Name.Length)
                              .ToArray();

      var allBcxFiles = directory
                        .Files.Where(
                            file => file.Extension == ".bck" ||
                                    file.Extension == ".bca")
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

    private IEnumerable<BmdModelFileBundle> ExtractPrimaryAndSecondaryModels_(
        IFileHierarchyDirectory directory,
        Func<IFileHierarchyFile, bool> primaryIdentifier
    ) {
      var bmdFiles = directory.Files.Where(
                                  file => file.Extension == ".bmd")
                              .ToArray();
      var bcxFiles = directory
                     .Files.Where(
                         file => file.Extension == ".bck" ||
                                 file.Extension == ".bca")
                     .ToArray();

      return this.ExtractPrimaryAndSecondaryModels_(primaryIdentifier,
                                                    bmdFiles,
                                                    bcxFiles);
    }

    private IEnumerable<BmdModelFileBundle> ExtractPrimaryAndSecondaryModels_(
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

    private IEnumerable<BmdModelFileBundle> ExtractModels_(
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
      });
  }
}