using fin.common;
using fin.io;
using fin.io.bundles;
using fin.util.progress;

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
            "fish",
            "peach");

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

    var bmdFiles
        = playerDirectory.FilesWithExtension(".bmd")
                     .Where(f => f.NameWithoutExtension is not
                                ("wario_metal_model"));
    var bcaFiles = playerDirectory.FilesWithExtension(".bca").ToArray();

    foreach (var bmdFile in bmdFiles) {
      organizer.Add(new Sm64dsModelFileBundle {
          GameName = "super_mario_64_ds",
          BmdFile = bmdFile,
      }.Annotate(bmdFile));
    }

    // wario_metal_model
    {
      var bmdFile = playerDirectory.AssertGetExistingFile("wario_metal_model.bmd");
      organizer.Add(new Sm64dsModelFileBundle {
          GameName = "super_mario_64_ds",
          BmdFile = bmdFile,
          BcaFiles = bcaFiles,
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