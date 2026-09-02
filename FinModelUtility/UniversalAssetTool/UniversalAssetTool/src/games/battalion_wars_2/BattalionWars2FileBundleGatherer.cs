using fin.config;
using fin.io;
using fin.io.archive;
using fin.io.bundles;
using fin.util.progress;

using modl.api;

using uni.games.battalion_wars_1;
using uni.util.io;

namespace uni.games.battalion_wars_2;

public sealed class BattalionWars2FileBundleGatherer
    : BWiiFileBundleGatherer {
  public override string Name => "battalion_wars_2";

  protected override void GatherFileBundlesFromHierarchy(
      IFileBundleOrganizer organizer,
      IMutablePercentageProgress mutablePercentageProgress,
      IFileHierarchy fileHierarchy) {
    var didUpdateAny = false;
    foreach (var directory in fileHierarchy) {
      var didUpdate = false;
      var resFiles =
          directory.GetExistingFiles()
                   .Where(file => file.Name.EndsWith(".res.gz"));
      foreach (var resFile in resFiles) {
        didUpdateAny |= didUpdate
            |= (ResArchiveImporter.Extract(resFile.Impl) ==
                ArchiveExtractionResult.NEWLY_EXTRACTED);
      }

      if (didUpdate) {
        directory.Refresh();
      }
    }

    if (didUpdateAny) {
      fileHierarchy.RefreshRootAndUpdateCache();
    }

    new FileHierarchyAssetBundleSeparator(
        fileHierarchy,
        (directory, organizer) => {
          var modlFiles = directory.FilesWithExtension(".modl");
          var animFiles = directory.FilesWithExtension(".anim");

          var svetModlFile =
              modlFiles.Where(modlFile =>
                                  modlFile.NameWithoutExtension is "SVET")
                       .ToHashSet();

          var gruntModlFiles =
              modlFiles.Where(modlFile =>
                                  modlFile.Name.EndsWith("G_HI_LOD.modl"))
                       .ToHashSet();
          var vetModlFiles =
              modlFiles.Where(modlFile =>
                                  modlFile.Name.EndsWith("V_HI_LOD.modl"))
                       .ToHashSet();

          var fvAnimFiles =
              animFiles.Where(animFile =>
                                  animFile.NameWithoutExtension
                                          .StartsWith("FV"))
                       .ToArray();
          var wgruntAnimFiles =
              animFiles.Where(animFile =>
                                  animFile.NameWithoutExtension.StartsWith(
                                      "WG"))
                       .ToArray();

          var otherModlFiles =
              modlFiles.Where(modlFile =>
                                  !svetModlFile.Contains(modlFile) &&
                                  !gruntModlFiles.Contains(modlFile) &&
                                  !vetModlFiles.Contains(modlFile)
                       )
                       .ToArray();

          var allModlsAndAnims =
              new (IEnumerable<IFileHierarchyFile>,
                  IReadOnlyList<IReadOnlyTreeFile>?
                  )
                  [] {
                      (svetModlFile, fvAnimFiles),
                      (gruntModlFiles, wgruntAnimFiles),
                      (vetModlFiles, fvAnimFiles),
                      (otherModlFiles, null),
                  };

          foreach (var (currModlFiles, currAnimFiles) in allModlsAndAnims) {
            foreach (var modlFile in currModlFiles) {
              organizer.Add(new ModlModelFileBundle {
                  ModlFile = modlFile,
                  GameVersion = GameVersion.BW2,
                  AnimFiles = currAnimFiles
              });
            }
          }

          foreach (var outFile in directory.GetExistingFiles()
                                           .Where(file => file.Name.EndsWith(
                                                      ".out.gz"))) {
            organizer.Add(new OutModelFileBundle {
                OutFile = outFile,
                GameVersion = GameVersion.BW2,
            });
          }

          if (directory.Name is "CompoundFiles") {
            var levelXmlFiles = directory
                                .FilesWithExtension(".xml")
                                .Where(file =>
                                           !file.NameWithoutExtension.EndsWith(
                                               "_Level"))
                                .Where(file =>
                                           !file.NameWithoutExtension
                                                .EndsWith("_preload"));
            foreach (var levelXmlFile in levelXmlFiles) {
              organizer.Add(new BwSceneFileBundle {
                  MainXmlFile = levelXmlFile,
                  GameVersion = GameVersion.BW2,
              });
            }
          }
        }
    ).GatherFileBundles(organizer, mutablePercentageProgress);
  }
}