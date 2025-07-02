using fin.common;
using fin.config;
using fin.data.sets;
using fin.io;
using fin.io.bundles;
using fin.util.progress;

using nitro.api;

using sm64ds.api;

using uni.platforms.ds;
using uni.util.bundles;
using uni.util.io;

namespace uni.games.super_mario_64_ds;

public class SuperMario64DsFileBundleGatherer : IAnnotatedFileBundleGatherer {
  private readonly IModelSeparator modelSeparator_
      = new ModelSeparator(directory => directory.Name)
        .Register(
            "basabasa",
            new ExactCasesMethod()
                .Case("basabasa.bmd", "basabasa_fly.bca")
                .Case("basabasa_wait.bmd", "basabasa_wait.bca"))
        .Register<AllAnimationsModelSeparatorMethod>(
            "bombhei",
            "door",
            "fish",
            "kuribo",
            "peach",
            "penguin")
        .Register("book", new PrimaryModelSeparatorMethod("book.bmd"))
        .Register("choropu", new PrimaryModelSeparatorMethod("choropu.bmd"))
        .Register(
            "donketu",
            new PrefixCasesMethod()
                .Case("boss_donketu", "donketu_")
                .Case("donketu", "donketu_")
                .Case("ice_donketu", "ice_donketu"))
        .Register("obj_hatena_box",
                  new PrimaryModelSeparatorMethod("hatena_karabox.bmd"))
        .Register(
            "nokonoko",
            new PrefixCasesMethod()
                .Case("nokonoko", "")
                .Case("nokonoko_red", ""))
        .Register("snowman",
                  new PrimaryModelSeparatorMethod("snowman_model.bmd"))
        .Register("star", new PrefixCasesMethod().Case("obj_star", ""))
        .Register("togezo", new PrimaryModelSeparatorMethod("togezo.bmd"))
        .Register("yurei_mucho",
                  new PrimaryModelSeparatorMethod("yurei_mucho.bmd"));

  public void GatherFileBundles(
      IFileBundleOrganizer organizer,
      IMutablePercentageProgress mutablePercentageProgress) {
    if (!DirectoryConstants.ROMS_DIRECTORY.TryToGetExistingFile(
            "super_mario_64_ds.nds",
            out var superMario64DsRom)) {
      return;
    }

    var fileHierarchy
        = new DsFileHierarchyExtractor().ExtractFromRom(superMario64DsRom);

    NarcArchiveImporter.ImportAndExtractAll(
        fileHierarchy,
        FinConfig.CleanUpArchives);

    new AnnotatedFileBundleGathererAccumulatorWithInput<IFileHierarchy>(
            fileHierarchy)
        .Add(this.GetAutomaticModels_)
        .Add(GetDsmtModels_)
        .Add(GetMgModels_)
        .Add(GetPlayerModels_)
        .Add(this.GetViaSeparator_)
        .GatherFileBundles(organizer, mutablePercentageProgress);
  }

  private void GetAutomaticModels_(IFileBundleOrganizer organizer,
                                   IFileHierarchy fileHierarchy) {
    foreach (var directory in fileHierarchy) {
      if (directory.Name is "DSMT" or "MG" or "player" ||
          this.modelSeparator_.Contains(directory)) {
        continue;
      }

      var bmdFiles = directory.GetFilesWithFileType(".bmd").ToArray();
      if (bmdFiles.Length == 0) {
        continue;
      }

      if (bmdFiles.Length == 1) {
        var bmdFile = bmdFiles[0];
        var bcaFiles = directory.GetFilesWithFileType(".bca").ToArray();
        organizer.Add(new Sm64dsModelFileBundle {
            GameName = "super_mario_64_ds",
            BmdFile = bmdFile,
            BcaFiles = bcaFiles,
        }.Annotate(bmdFile));
      } else {
        foreach (var bmdFile in bmdFiles) {
          organizer.Add(new Sm64dsModelFileBundle {
              GameName = "super_mario_64_ds",
              BmdFile = bmdFile,
          }.Annotate(bmdFile));
        }
      }
    }
  }

  private static void GetDsmtModels_(IFileBundleOrganizer organizer,
                                     IFileHierarchy fileHierarchy) {
    var dsmtDirectory
        = fileHierarchy.Root.AssertGetExistingSubdir("data/data/DSMT");

    var dsmtBcas = dsmtDirectory.FilesWithExtension(".bca").ToArray();

    // Mario
    {
      var marioBmd = dsmtDirectory.AssertGetExistingFile("face_demo_mario.bmd");
      organizer.Add(new Sm64dsModelFileBundle {
          GameName = "super_mario_64_ds",
          BmdFile = marioBmd,
          BcaFiles = dsmtBcas
                     .Where(f => f.NameWithoutExtension.EndsWith("mario"))
                     .ToArray(),
      }.Annotate(marioBmd));
    }

    // Star
    {
      var marioBmd
          = dsmtDirectory.AssertGetExistingFile("face_demo_mariostar.bmd");
      organizer.Add(new Sm64dsModelFileBundle {
          GameName = "super_mario_64_ds",
          BmdFile = marioBmd,
          BcaFiles = dsmtBcas
                     .Where(f => f.NameWithoutExtension.EndsWith("star"))
                     .ToArray(),
      }.Annotate(marioBmd));
    }

    // Yoshi
    {
      var marioBmd = dsmtDirectory.AssertGetExistingFile("face_demo_yoshi.bmd");
      organizer.Add(new Sm64dsModelFileBundle {
          GameName = "super_mario_64_ds",
          BmdFile = marioBmd,
          BcaFiles = dsmtBcas
                     .Where(f => f.NameWithoutExtension.EndsWith("yoshi"))
                     .ToArray(),
      }.Annotate(marioBmd));
    }
  }

  private static void GetMgModels_(IFileBundleOrganizer organizer,
                                   IFileHierarchy fileHierarchy) {
    var mgDirectory
        = fileHierarchy.Root.AssertGetExistingSubdir("data/MG");

    var bmdFiles
        = mgDirectory.FilesWithExtension(".bmd")
                     .Where(f => f.NameWithoutExtension is not
                                ("esp_card"
                                 or "esp_hamon"
                                 or "kino_d"
                                 or "luigi_d"
                                 or "yoshi_model"));
    var bcaFiles = mgDirectory.FilesWithExtension(".bca").ToArray();

    foreach (var bmdFile in bmdFiles) {
      organizer.Add(new Sm64dsModelFileBundle {
          GameName = "super_mario_64_ds",
          BmdFile = bmdFile,
      }.Annotate(bmdFile));
    }

    // esp_card
    {
      var bmdFile = mgDirectory.AssertGetExistingFile("esp_card.bmd");
      organizer.Add(new Sm64dsModelFileBundle {
          GameName = "super_mario_64_ds",
          BmdFile = bmdFile,
          BcaFiles = bcaFiles.Where(f => f.Name.StartsWith("esp_card_"))
                             .ToArray(),
      }.Annotate(bmdFile));
    }

    // esp_hamon
    {
      var bmdFile = mgDirectory.AssertGetExistingFile("esp_hamon.bmd");
      organizer.Add(new Sm64dsModelFileBundle {
          GameName = "super_mario_64_ds",
          BmdFile = bmdFile,
          BcaFiles = bcaFiles.Where(f => f.Name.StartsWith("esp_hamon"))
                             .ToArray(),
      }.Annotate(bmdFile));
    }

    // kino_d
    {
      var bmdFile = mgDirectory.AssertGetExistingFile("kino_d.bmd");
      organizer.Add(new Sm64dsModelFileBundle {
          GameName = "super_mario_64_ds",
          BmdFile = bmdFile,
          BcaFiles = bcaFiles.Where(f => f.Name.StartsWith("kino_"))
                             .ToArray(),
      }.Annotate(bmdFile));
    }

    // luigi_d
    {
      var bmdFile = mgDirectory.AssertGetExistingFile("luigi_d.bmd");
      organizer.Add(new Sm64dsModelFileBundle {
          GameName = "super_mario_64_ds",
          BmdFile = bmdFile,
          BcaFiles = bcaFiles.Where(f => f.Name.StartsWith("luigi_d_"))
                             .ToArray(),
      }.Annotate(bmdFile));
    }

    // yoshi_model
    {
      var bmdFile = mgDirectory.AssertGetExistingFile("yoshi_model.bmd");
      organizer.Add(new Sm64dsModelFileBundle {
          GameName = "super_mario_64_ds",
          BmdFile = bmdFile,
          BcaFiles = bcaFiles.Where(f => f.Name.StartsWith("yoshi_")).ToArray(),
      }.Annotate(bmdFile));
    }
  }

  private static void GetPlayerModels_(IFileBundleOrganizer organizer,
                                       IFileHierarchy fileHierarchy) {
    var playerDirectory
        = fileHierarchy.Root.AssertGetExistingSubdir("data/data/player");

    var bcaSplitter = playerDirectory.FilesWithExtension(".bca").SplitByName();
    var bmdSplitter = playerDirectory.FilesWithExtension(".bmd").SplitByName();

    var balloonMarioBmd = bmdSplitter.Matching("b_mario_all.bmd");
    organizer.Add(new Sm64dsModelFileBundle {
        GameName = "super_mario_64_ds",
        BmdFile = balloonMarioBmd,
        BcaFiles = [bcaSplitter.Matching("b_mario_start.bca")],
    }.Annotate(balloonMarioBmd));

    var wingBmd = bmdSplitter.Matching("wing_model.bmd");
    organizer.Add(new Sm64dsModelFileBundle {
        GameName = "super_mario_64_ds",
        BmdFile = wingBmd,
        BcaFiles = [bcaSplitter.Matching("wing_flutter.bca")],
    }.Annotate(wingBmd));

    var luigiBcas = bcaSplitter.StartsWith("L_");
    var marioBcas = bcaSplitter.StartsWith("M_");
    var warioBcas = bcaSplitter.StartsWith("W_");
    var yoshiBcas = bcaSplitter.StartsWith("Y_");
    var commonBcas = bcaSplitter.Remaining();

    var luigiBmd = bmdSplitter.Matching("luigi_model.bmd");
    var marioBmd = bmdSplitter.Matching("mario_model.bmd");
    var warioBmds = new[] {
        bmdSplitter.Matching("wario_model.bmd"),
        bmdSplitter.Matching("wario_metal_model.bmd")
    };
    var yoshiBmd = bmdSplitter.Matching("yoshi_model.bmd");

    // Luigi
    organizer.Add(new Sm64dsModelFileBundle {
        GameName = "super_mario_64_ds",
        BmdFile = luigiBmd,
        BcaFiles = luigiBcas.Concat(commonBcas).ToArray(),
    }.Annotate(luigiBmd));

    // Mario
    organizer.Add(new Sm64dsModelFileBundle {
        GameName = "super_mario_64_ds",
        BmdFile = marioBmd,
        BcaFiles = marioBcas.Concat(commonBcas).ToArray(),
    }.Annotate(marioBmd));

    // Wario
    {
      var allWarioBcas = warioBcas.Concat(commonBcas).ToArray();
      foreach (var warioBmd in warioBmds) {
        organizer.Add(new Sm64dsModelFileBundle {
            GameName = "super_mario_64_ds",
            BmdFile = warioBmd,
            BcaFiles = allWarioBcas,
        }.Annotate(warioBmd));
      }
    }

    // Yoshi
    organizer.Add(new Sm64dsModelFileBundle {
        GameName = "super_mario_64_ds",
        BmdFile = yoshiBmd,
        BcaFiles = yoshiBcas.Concat(commonBcas).ToArray(),
    }.Annotate(yoshiBmd));

    // Other models
    foreach (var bmdFile in bmdSplitter.Remaining()) {
      organizer.Add(new Sm64dsModelFileBundle {
          GameName = "super_mario_64_ds",
          BmdFile = bmdFile,
      }.Annotate(bmdFile));
    }
  }

  private void GetViaSeparator_(IFileBundleOrganizer tOrganizer,
                                IMutablePercentageProgress progress,
                                IFileHierarchy fileHierarchy)
    => new FileHierarchyAssetBundleSeparator(
        fileHierarchy,
        (subdir, organizer) => {
          if (!this.modelSeparator_.Contains(subdir)) {
            return;
          }

          var bmdFiles = subdir.FilesWithExtensionsRecursive(".bmd").ToArray();
          if (bmdFiles.Length == 0) {
            return;
          }

          var bcaFiles = subdir.FilesWithExtensionsRecursive(".bca").ToArray();

          try {
            foreach (var bundle in this.modelSeparator_.Separate(
                         subdir,
                         bmdFiles,
                         bcaFiles)) {
              organizer.Add(new Sm64dsModelFileBundle {
                  GameName = "super_mario_64_ds",
                  BmdFile = bundle.ModelFile,
                  BcaFiles = bundle.AnimationFiles.ToArray(),
              }.Annotate(bundle.ModelFile));
            }
          } catch { }
        }
    ).GatherFileBundles(tOrganizer, progress);
}