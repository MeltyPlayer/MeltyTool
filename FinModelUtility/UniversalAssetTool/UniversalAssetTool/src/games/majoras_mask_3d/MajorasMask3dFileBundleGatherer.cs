using grezzo.api;

using fin.io;
using fin.io.bundles;

using uni.platforms.threeDs;
using uni.util.bundles;
using uni.util.io;

using fin.util.progress;

namespace uni.games.majoras_mask_3d;

public class MajorasMask3dFileBundleGatherer : IAnnotatedFileBundleGatherer {
  private readonly IModelSeparator separator_
      = new ModelSeparator(directory => directory.Name)
        .Register(new AllAnimationsModelSeparatorMethod(),
                  "zelda_cow",
                  "zelda2_jso")
        .Register(new SameNameSeparatorMethod(), "zelda2_zoraband")
        .Register(new PrimaryModelSeparatorMethod("eyegoal.cmb"),
                  "zelda2_eg")
        .Register("zelda2_boss_hakugin",
                  new PrefixCasesMethod()
                      .Case("goat", "iceb_")
                      .Case("boss_hakugin_lightball_",
                            "boss_hakugin_lightball_"))
        .Register("zelda2_boss01",
                  new PrefixCasesMethod()
                      .Case("mbug", "mbug_")
                      .Case("moth", "moth_")
                      .Case("weakpoint", "weakpoint_")
                      .Rest("odoruwa.cmb"))
        .Register("zelda2_boss02",
                  new PrefixCasesMethod()
                      .Case("sand_shape01", "sand_shape01")
                      .Case("sand_shape02", "sand_shape02")
                      .Rest("minimold"))
        .Register("zelda2_boss03",
                  new PrefixCasesMethod()
                      .Case("boss03_spike_2", "boss03_spike_2")
                      .Case("boss03_wave", "boss03_wave")
                      .Case("minibus", "minibus_")
                      .Case("sand_shape02", "sand_shape02")
                      .Rest("gujorg"))
        .Register("zelda2_boss04", new PrimaryModelSeparatorMethod("wort.cmb"));


  public void GatherFileBundles(
      IFileBundleOrganizer organizer,
      IMutablePercentageProgress mutablePercentageProgress) {
    if (!new ThreeDsFileHierarchyExtractor().TryToExtractFromGame(
            "majoras_mask_3d",
            out var fileHierarchy)) {
      return;
    }

    new AnnotatedFileBundleGathererAccumulatorWithInput<IFileHierarchy>(
            fileHierarchy)
        .Add(this.GetAutomaticModels_)
        .Add(this.GetModelsViaSeparator_)
        .Add(this.GetLinkModels_)
        .Add(this.GetTwinmoldModels_)
        .GatherFileBundles(organizer, mutablePercentageProgress);
  }

  private void GetAutomaticModels_(
      IFileBundleOrganizer organizer,
      IFileHierarchy fileHierarchy) {
    var actorsDir = fileHierarchy.Root.AssertGetExistingSubdir("actors");
    foreach (var actorDir in actorsDir.GetExistingSubdirs()) {
      var actorDirName = actorDir.Name;
      if (actorDirName.StartsWith("zelda2_link_") ||
          actorDirName.StartsWith("zelda2_boss02_") ||
          this.separator_.Contains(actorDir)) {
        continue;
      }

      var animations =
          actorDir.FilesWithExtensionRecursive(".csab").ToArray();
      var models = actorDir.FilesWithExtensionRecursive(".cmb").ToArray();

      if (models.Length == 1 || animations.Length == 0) {
        foreach (var model in models) {
          organizer.Add(new CmbModelFileBundle(
                            model,
                            animations,
                            null,
                            null).Annotate(model));
        }
      } else {
        foreach (var model in models) {
          organizer.Add(new CmbModelFileBundle(
                            model,
                            null,
                            null,
                            null).Annotate(model));
        }
      }
    }

    var sceneDir = fileHierarchy.Root.AssertGetExistingSubdir("scenes");
    foreach (var zsiFile in sceneDir.GetFilesWithFileType(".zsi")) {
      organizer.Add(new ZsiSceneFileBundle(zsiFile)
                        .Annotate(zsiFile));
    }
  }


  private void GetModelsViaSeparator_(IFileBundleOrganizer organizer,
                                      IMutablePercentageProgress progress,
                                      IFileHierarchy fileHierarchy)
    => new FileHierarchyAssetBundleSeparator(
        fileHierarchy,
        (subdir, organizer) => {
          if (!this.separator_.Contains(subdir)) {
            return;
          }

          var cmbFiles =
              subdir.FilesWithExtensionsRecursive(".cmb").ToArray();
          if (cmbFiles.Length == 0) {
            return;
          }

          var csabFiles =
              subdir.FilesWithExtensionsRecursive(".csab").ToArray();
          var ctxbFiles =
              subdir.FilesWithExtensionsRecursive(".ctxb").ToArray();

          try {
            foreach (var bundle in this.separator_.Separate(
                         subdir,
                         cmbFiles,
                         csabFiles)) {
              organizer.Add(new CmbModelFileBundle(
                                bundle.ModelFile,
                                bundle.AnimationFiles.ToArray(),
                                ctxbFiles,
                                null
                            ).Annotate(bundle.ModelFile));
            }
          } catch { }
        }
    ).GatherFileBundles(organizer, progress);

  private void GetLinkModels_(IFileBundleOrganizer organizer,
                              IFileHierarchy fileHierarchy) {
    var actorsDir = fileHierarchy.Root.AssertGetExistingSubdir("actors");

    var modelsAndAnimations = new[] {
        ("zelda2_link_boy_new/boy/model/link_demon.cmb", "boy"),
        ("zelda2_link_child_new/child/model/link_child.cmb", "child"),
        ("zelda2_link_goron_new/goron/model/link_goron.cmb", "goron"),
        ("zelda2_link_nuts_new/nuts/model/link_deknuts.cmb", "nuts"),
        ("zelda2_link_zora_new/zora/model/link_zora.cmb", "zora"),
    };

    foreach (var (modelPath, animationDir) in modelsAndAnimations) {
      var cmbFile
          = actorsDir.AssertGetExistingFile(
              modelPath);
      var csabFiles = fileHierarchy
                      .Root.AssertGetExistingSubdir(
                          $"actors/zelda2_link_new/{animationDir}/anim")
                      .FilesWithExtension(".csab")
                      .ToArray();

      organizer.Add(new CmbModelFileBundle(
                        cmbFile,
                        csabFiles).Annotate(cmbFile));
    }
  }

  private void GetTwinmoldModels_(IFileBundleOrganizer organizer,
                                  IFileHierarchy fileHierarchy) {
    var actorsDir = fileHierarchy.Root.AssertGetExistingSubdir("actors");

    var models = new[] {
        actorsDir.AssertGetExistingSubdir("zelda2_boss02_blue")
                 .FilesWithExtensionRecursive(".cmb")
                 .Single(),
        actorsDir.AssertGetExistingSubdir("zelda2_boss02_red")
                 .FilesWithExtensionRecursive(".cmb")
                 .Single()
    };
    var animations = actorsDir.AssertGetExistingSubdir("zelda2_boss02_anim")
                              .FilesWithExtensionRecursive(".csab")
                              .ToArray();

    foreach (var model in models) {
      organizer.Add(new CmbModelFileBundle(
                        model,
                        animations).Annotate(model));
    }
  }
}