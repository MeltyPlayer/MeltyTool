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
                  "zelda2_eg");


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
        .GatherFileBundles(organizer, mutablePercentageProgress);
  }

  private void GetAutomaticModels_(
      IFileBundleOrganizer organizer,
      IFileHierarchy fileHierarchy) {
    var actorsDir = fileHierarchy.Root.AssertGetExistingSubdir("actors");
    foreach (var actorDir in actorsDir.GetExistingSubdirs()) {
      if (actorDir.Name.StartsWith("zelda2_link_") ||
                                   this.separator_.Contains(actorDir)) {
        continue;
      }

      var animations =
          actorDir.FilesWithExtensionRecursive(".csab").ToArray();
      var models = actorDir.FilesWithExtensionRecursive(".cmb").ToArray();

      if (models.Length == 1 || animations.Length == 0) {
        foreach (var model in models) {
          organizer.Add(new CmbModelFileBundle(
                            "majoras_mask_3d",
                            model,
                            animations,
                            null,
                            null).Annotate(model));
        }
      } else {
        foreach (var model in models) {
          organizer.Add(new CmbModelFileBundle(
                            "majoras_mask_3d",
                            model,
                            null,
                            null,
                            null).Annotate(model));
        }
      }
    }

    var sceneDir = fileHierarchy.Root.AssertGetExistingSubdir("scenes");
    foreach (var zsiFile in sceneDir.GetFilesWithFileType(".zsi")) {
      organizer.Add(new ZsiSceneFileBundle("majoras_mask_3d", zsiFile)
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
                                "majoras_mask_3d",
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
                        "majoras_mask_3d",
                        cmbFile,
                        csabFiles,
                        null,
                        null).Annotate(cmbFile));
    }
  }
}