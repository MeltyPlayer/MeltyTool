using uni.platforms;
using uni.platforms.threeDs;
using uni.util.io;
using uni.util.bundles;

using cmb.api;

using fin.io;
using fin.io.bundles;


namespace uni.games.ocarina_of_time_3d {
  public class OcarinaOfTime3dFileGatherer
      : IFileBundleGatherer<CmbModelFileBundle> {
    // TODO: Add support for Link
    // TODO: Add support for faceb
    // TODO: Add support for cmab
    // TODO: Add support for other texture types

    // *sigh*
    // Why tf did they have to name things so randomly????
    private readonly IModelSeparator separator_
        = new ModelSeparator(directory => directory.Name)
          .Register(new AllAnimationsModelSeparatorMethod(),
                    "zelda_cow",
                    "zelda_rd",
                    "zelda_tr",
                    "zelda_zf"
          )
          // TODO: This is probably wrong
          .Register("zelda_box",
                    new NoAnimationsModelSeparatorMethod()
              /*new ExactCasesMethod()
                  .Case("demo_tre_lgt_mdl_info.cmb",
                        "demo_tre_lgt_c_fcurve_data.csab",
                        "demo_tre_lgt_fcurve_data.csab")
                  .Case("tr_box.cmb",
                        "cdemo_box_boxA.csab",
                        "demo_box_boxA.csab",
                        "demo_box_boxB.csab")*/)
          // TODO: This is *definitely* wrong
          .Register("zelda_bv",
                    new NoAnimationsModelSeparatorMethod()
              /*new PrefixCasesMethod()
                  .Case("balinadearm", "bva_", "bv_arm_")
                  .Case("bve_model", "bve_")
                  .Case("balinadecore", "bvc_")
                  .Case("efc_bari_model", "bvb_")
                  .Case("bv_inazumaMINI2_modelT", "bl2")*/)
          .Register("zelda_bxa",
                    new ExactCasesMethod()
                        .Case("balinadearm.cmb",
                              "tentacle_motion_test01.csab",
                              "baarm_death.csab")
                        .Case("balinadetrap.cmb", "balinadetrap.csab"))
          // TODO: Figure these all out
          .Register("zelda_dekubaba",
                    new PrimaryModelSeparatorMethod("dekubaba.cmb"))
          .Register("zelda_dekunuts",
                    new PrimaryModelSeparatorMethod("okorinuts.cmb"))
          .Register("zelda_dh", new NoAnimationsModelSeparatorMethod())
          .Register("zelda_dnk",
                    new PrimaryModelSeparatorMethod("choronuts.cmb"))
          .Register("zelda_dns",
                    new PrimaryModelSeparatorMethod("eldernuts.cmb"))
          .Register("zelda_dy_obj", new NoAnimationsModelSeparatorMethod())
          .Register("zelda_ec", new NoAnimationsModelSeparatorMethod())
          .Register("zelda_ec2", new NoAnimationsModelSeparatorMethod())
          .Register("zelda_efc_tw", new NoAnimationsModelSeparatorMethod())
          .Register("zelda_fantomHG", new NoAnimationsModelSeparatorMethod())
          .Register("zelda_fd", new NoAnimationsModelSeparatorMethod())
          .Register("zelda_field_keep", new NoAnimationsModelSeparatorMethod())
          .Register("zelda_fishing", new NoAnimationsModelSeparatorMethod())
          .Register("zelda_fr", new NoAnimationsModelSeparatorMethod())
          .Register("zelda_fw", new NoAnimationsModelSeparatorMethod())
          .Register("zelda_ganon2",
                    new PrimaryModelSeparatorMethod("ganon.cmb"))
          .Register("zelda_ganon_down", new NoAnimationsModelSeparatorMethod())
          .Register("zelda_gnd",
                    new PrimaryModelSeparatorMethod("phantomganon.cmb"))
          .Register("zelda_gndd",
                    new PrimaryModelSeparatorMethod("ganondorfchild.cmb"))
          .Register("zelda_goma", new PrimaryModelSeparatorMethod("goma.cmb"))
          .Register("zelda_haka_door", new NoAnimationsModelSeparatorMethod())
          .Register("zelda_hidan_objects",
                    new NoAnimationsModelSeparatorMethod())
          .Register("zelda_hintnuts",
                    new PrimaryModelSeparatorMethod("dekunuts.cmb"))
          .Register("zelda_ik", new NoAnimationsModelSeparatorMethod())
          .Register("zelda_kdodongo",
                    new PrimaryModelSeparatorMethod("kingdodongo.cmb"))
          .Register("zelda_mag", new NoAnimationsModelSeparatorMethod())
          .Register("zelda_mb", new NoAnimationsModelSeparatorMethod())
          .Register("zelda_mizu_objects",
                    new NoAnimationsModelSeparatorMethod())
          .Register("zelda_mu", new NoAnimationsModelSeparatorMethod())
          .Register("zelda_nw", new NoAnimationsModelSeparatorMethod())
          .Register("zelda_oc2", new NoAnimationsModelSeparatorMethod())
          .Register("zelda_oF1d", new NoAnimationsModelSeparatorMethod())
          .Register("zelda_ph", new NoAnimationsModelSeparatorMethod())
          .Register("zelda_po", new NoAnimationsModelSeparatorMethod())
          .Register("zelda_po_composer",
                    new PrimaryModelSeparatorMethod("pohmusic.cmb"))
          .Register("zelda_po_field", new NoAnimationsModelSeparatorMethod())
          .Register("zelda_po_sisters", new NoAnimationsModelSeparatorMethod())
          .Register("zelda_ps", new NoAnimationsModelSeparatorMethod())
          .Register("zelda_sd", new NoAnimationsModelSeparatorMethod())
          .Register("zelda_shopnuts", new NoAnimationsModelSeparatorMethod())
          .Register("zelda_skj", new NoAnimationsModelSeparatorMethod())
          .Register("zelda_spot02_objects",
                    new NoAnimationsModelSeparatorMethod())
          .Register("zelda_sst", new NoAnimationsModelSeparatorMethod())
          .Register("zelda_st", new NoAnimationsModelSeparatorMethod())
          .Register("zelda_tw", new NoAnimationsModelSeparatorMethod())
          .Register("zelda_vali", new NoAnimationsModelSeparatorMethod())
          .Register("zelda_wm2", new NoAnimationsModelSeparatorMethod())
          .Register("zelda_xc", new NoAnimationsModelSeparatorMethod());

    public IEnumerable<CmbModelFileBundle> GatherFileBundles(bool assert) {
      if (!DirectoryConstants.ROMS_DIRECTORY.PossiblyAssertExistingFile(
              "ocarina_of_time_3d.cia",
              assert,
              out var ocarinaOfTime3dRom)) {
        return Enumerable.Empty<CmbModelFileBundle>();
      }

      var fileHierarchy =
          new ThreeDsFileHierarchyExtractor()
              .ExtractFromRom(ocarinaOfTime3dRom);

      return new FileBundleGathererAccumulatorWithInput<CmbModelFileBundle,
                 IFileHierarchy>(
                 fileHierarchy)
             .Add(this.GetAutomaticModels_)
             .Add(this.GetModelsViaSeparator_)
             .Add(this.GetLinkModels_)
             .Add(this.GetGanondorfModels_)
             .Add(this.GetOwlModels_)
             .GatherFileBundles(assert);
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
                                        "ocarina_of_time_3d",
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

    private IEnumerable<CmbModelFileBundle> GetAutomaticModels_(
        IFileHierarchy fileHierarchy) {
      var actorsDir = fileHierarchy.Root.GetExistingSubdir("actor");

      foreach (var actorDir in actorsDir.Subdirs) {
        var animations =
            actorDir.FilesWithExtensionRecursive(".csab").ToArray();
        var models = actorDir.FilesWithExtensionRecursive(".cmb").ToArray();

        if (models.Length == 1 || animations.Length == 0) {
          foreach (var model in models) {
            yield return new CmbModelFileBundle(
                "ocarina_of_time_3d",
                model,
                animations,
                null,
                null);
          }
        }
      }
    }

    private IEnumerable<CmbModelFileBundle> GetLinkModels_(
        IFileHierarchy fileHierarchy) {
      var actorsDir = fileHierarchy.Root.GetExistingSubdir("actor");

      var childDir = actorsDir.GetExistingSubdir("zelda_link_child_new/child");
      yield return new CmbModelFileBundle(
          "ocarina_of_time_3d",
          childDir.GetExistingFile("model/childlink_v2.cmb"),
          childDir.GetExistingSubdir("anim")
                  .FilesWithExtension(".csab")
                  .ToArray(),
          null,
          null);

      var adultDir = actorsDir.GetExistingSubdir("zelda_link_boy_new/boy");
      yield return new CmbModelFileBundle(
          "ocarina_of_time_3d",
          adultDir.GetExistingFile("model/link_v2.cmb"),
          adultDir.GetExistingSubdir("anim")
                  .FilesWithExtension(".csab")
                  .ToArray(),
          null,
          null);
    }

    private IEnumerable<CmbModelFileBundle> GetGanondorfModels_(
        IFileHierarchy fileHierarchy) {
      var baseDir = fileHierarchy.Root.GetExistingSubdir("actor/zelda_ganon");

      var modelDir = baseDir.GetExistingSubdir("Model");

      var allAnimations =
          baseDir.GetExistingSubdir("Anim").Files;
      var capeAnimations =
          allAnimations.Where(file => file.Name.EndsWith("_m.csab"));
      var ganondorfAnimations =
          allAnimations.Where(file => !capeAnimations.Contains(file));

      yield return new CmbModelFileBundle(
          "ocarina_of_time_3d",
          modelDir.GetExistingFile("ganondorf.cmb"),
          ganondorfAnimations.ToArray(),
          null,
          null);
      yield return new CmbModelFileBundle(
          "ocarina_of_time_3d",
          modelDir.GetExistingFile("ganon_mant_model.cmb"),
          capeAnimations.ToArray(),
          null,
          null);

      foreach (var otherModel in modelDir.Files.Where(
                   file => file.Name is not "ganondorf.cmb"
                                        or "ganon_mant_model.cmb")) {
        yield return new CmbModelFileBundle(
            "ocarina_of_time_3d",
            otherModel,
            null,
            null,
            null);
      }
    }

    private IEnumerable<CmbModelFileBundle> GetOwlModels_(
        IFileHierarchy fileHierarchy) {
      var owlDir = fileHierarchy.Root.GetExistingSubdir("actor/zelda_owl");

      // Waiting
      yield return new CmbModelFileBundle(
          "ocarina_of_time_3d",
          owlDir.GetExistingFile("Model/kaeporagaebora1.cmb"),
          owlDir.GetExistingSubdir("Anim")
                .FilesWithExtension(".csab")
                .Where(file => file.Name == "owl_wait.csab")
                .ToArray(),
          null,
          null);


      // Flying
      yield return new CmbModelFileBundle(
          "ocarina_of_time_3d",
          owlDir.GetExistingFile("Model/kaeporagaebora2.cmb"),
          owlDir.GetExistingSubdir("Anim")
                .FilesWithExtension(".csab")
                .Where(file => file.Name != "owl_wait.csab")
                .ToArray(),
          null,
          null);
    }
  }
}