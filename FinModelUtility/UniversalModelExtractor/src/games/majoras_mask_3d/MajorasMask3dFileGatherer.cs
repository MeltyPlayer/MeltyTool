using uni.platforms;
using uni.platforms.threeDs;
using uni.util.io;
using uni.util.bundles;

using cmb.api;

using fin.io;
using fin.io.bundles;


namespace uni.games.majoras_mask_3d {
  public class MajorasMask3dFileGatherer
      : IFileBundleGatherer<CmbModelFileBundle> {
    private readonly IModelSeparator separator_
        = new ModelSeparator(directory => directory.Name)
          .Register(new SameNameSeparatorMethod(), "zelda2_zoraband")
          .Register(new AllAnimationsModelSeparatorMethod(),
                    "zelda_cow");


    public IEnumerable<CmbModelFileBundle> GatherFileBundles(
        bool assert) {
      if (!DirectoryConstants.ROMS_DIRECTORY.PossiblyAssertExistingFile(
              "majoras_mask_3d.cia",
              assert,
              out var majorasMask3dRom)) {
        return Enumerable.Empty<CmbModelFileBundle>();
      }

      var fileHierarchy =
          new ThreeDsFileHierarchyExtractor()
              .ExtractFromRom(majorasMask3dRom);

      return new FileBundleGathererAccumulatorWithInput<CmbModelFileBundle,
                 IFileHierarchy>(fileHierarchy)
             .Add(this.GetAutomaticModels_)
             .Add(this.GetModelsViaSeparator_)
             .Add(this.GetLinkModels_)
             .GatherFileBundles(assert);
    }

    private IEnumerable<CmbModelFileBundle> GetAutomaticModels_(
        IFileHierarchy fileHierarchy) {
      var actorsDir = fileHierarchy.Root.GetExistingSubdir("actors");

      foreach (var actorDir in actorsDir.Subdirs) {
        if (actorDir.Name.StartsWith("zelda2_link_")) {
          continue;
        }

        var animations =
            actorDir.FilesWithExtensionRecursive(".csab").ToArray();
        var models = actorDir.FilesWithExtensionRecursive(".cmb").ToArray();

        if (models.Length == 1 || animations.Length == 0) {
          foreach (var model in models) {
            yield return new CmbModelFileBundle(
                "majoras_mask_3d",
                model,
                animations,
                null,
                null);
          }
        }
      }
    }


    private IEnumerable<CmbModelFileBundle> GetModelsViaSeparator_(
        IFileHierarchy fileHierarchy)
      => new FileHierarchyAssetBundleSeparator<CmbModelFileBundle>(
          fileHierarchy,
          subdir => {
            if (!separator_.Contains(subdir)) {
              return Enumerable.Empty<CmbModelFileBundle>();
            }

            var cmbFiles =
                subdir.FilesWithExtensionsRecursive(".cmb").ToArray();
            if (cmbFiles.Length == 0) {
              return Enumerable.Empty<CmbModelFileBundle>();
            }

            var csabFiles =
                subdir.FilesWithExtensionsRecursive(".csab").ToArray();
            var ctxbFiles =
                subdir.FilesWithExtensionsRecursive(".ctxb").ToArray();

            try {
              var bundles =
                  this.separator_.Separate(subdir, cmbFiles, csabFiles);
              return bundles.Select(bundle => new CmbModelFileBundle(
                                        "majoras_mask_3d",
                                        bundle.ModelFile,
                                        bundle.AnimationFiles.ToArray(),
                                        ctxbFiles,
                                        null
                                    ));
            } catch {
              return Enumerable.Empty<CmbModelFileBundle>();
            }
          }
      ).GatherFileBundles(false);

    private IEnumerable<CmbModelFileBundle> GetLinkModels_(
        IFileHierarchy fileHierarchy) {
      var actorsDir = fileHierarchy.Root.GetExistingSubdir("actors");

      var cmbFiles =
          new[] {
              actorsDir.GetExistingFile("zelda2_link_child_new/link_child.cmb"),
              actorsDir.GetExistingFile("zelda2_link_goron_new/link_goron.cmb"),
              actorsDir.GetExistingFile(
                  "zelda2_link_nuts_new/link_deknuts.cmb"),
              actorsDir.GetExistingFile("zelda2_link_zora_new/link_zora.cmb"),
          };

      var csabFiles = fileHierarchy
                      .Root.GetExistingSubdir("actors/zelda2_link_new")
                      .FilesWithExtension(".csab")
                      .ToArray();

      return cmbFiles.Select(cmbFile => new CmbModelFileBundle(
                                 "majoras_mask_3d",
                                 cmbFile,
                                 csabFiles,
                                 null,
                                 null));
    }
  }
}