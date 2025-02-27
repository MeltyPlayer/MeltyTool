using fin.io;
using fin.io.bundles;
using fin.util.progress;

using modl.api;

using uni.platforms.gcn;
using uni.util.io;

namespace uni.games.battalion_wars_1;

public class BattalionWars1FileBundleGatherer : IAnnotatedFileBundleGatherer {
  public void GatherFileBundles(
      IFileBundleOrganizer organizer,
      IMutablePercentageProgress mutablePercentageProgress) {
    if (!new GcnFileHierarchyExtractor().TryToExtractFromGame(
            "battalion_wars_1",
            out var fileHierarchy)) {
      return;
    }

    var didUpdateAny = false;
    foreach (var directory in fileHierarchy) {
      var didUpdate = false;
      var resFiles = directory.FilesWithExtension(".res");
      foreach (var resFile in resFiles) {
        didUpdateAny |= didUpdate |= new ResDump().Run(resFile);
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
          var sgruntModlFile =
              modlFiles.Where(modlFile =>
                                  modlFile.NameWithoutExtension is "SGRUNT")
                       .ToHashSet();

          var tvetModlFile =
              modlFiles.Where(modlFile =>
                                  modlFile.NameWithoutExtension is "TVET")
                       .ToHashSet();
          var tgruntModlFile =
              modlFiles.Where(modlFile =>
                                  modlFile.NameWithoutExtension is "TGRUNT")
                       .ToHashSet();

          var uvetModlFile =
              modlFiles.Where(modlFile =>
                                  modlFile.NameWithoutExtension is "UVET")
                       .ToHashSet();
          var ugruntModlFile =
              modlFiles.Where(modlFile =>
                                  modlFile.NameWithoutExtension is "UGRUNT")
                       .ToHashSet();

          var wvetModlFile =
              modlFiles.Where(modlFile =>
                                  modlFile.NameWithoutExtension is "WVET")
                       .ToHashSet();
          var wgruntModlFile =
              modlFiles.Where(modlFile =>
                                  modlFile.NameWithoutExtension is "WGRUNT")
                       .ToHashSet();

          var xvetModlFile =
              modlFiles.Where(modlFile =>
                                  modlFile.NameWithoutExtension is "XVET")
                       .ToHashSet();
          var xgruntModlFile =
              modlFiles.Where(modlFile =>
                                  modlFile.NameWithoutExtension is "XGRUNT")
                       .ToHashSet();


          var fvAnimFiles =
              animFiles.Where(
                           animFile =>
                               animFile.NameWithoutExtension.StartsWith("FV"))
                       .ToArray();
          var fgAnimFiles =
              animFiles.Where(
                           animFile =>
                               animFile.NameWithoutExtension.StartsWith("FG"))
                       .ToArray();

          var sgAnimFiles =
              animFiles.Where(
                           animFile =>
                               animFile.NameWithoutExtension.StartsWith(
                                   "SG"))
                       .ToArray();
          var uvAnimFiles =
              animFiles.Where(
                           animFile =>
                               animFile.NameWithoutExtension.StartsWith(
                                   "UV"))
                       .ToArray();
          var wgruntAnimFiles =
              animFiles.Where(
                           animFile =>
                               animFile.NameWithoutExtension.StartsWith(
                                   "WGRUNT"))
                       .ToArray();
          var xgAnimFiles =
              animFiles.Where(
                           animFile =>
                               animFile.NameWithoutExtension.StartsWith(
                                   "XG"))
                       .ToArray();
          var xvAnimFiles =
              animFiles.Where(
                           animFile =>
                               animFile.NameWithoutExtension.StartsWith(
                                   "XV"))
                       .ToArray();

          var otherModlFiles =
              modlFiles.Where(
                           modlFile =>
                               !sgruntModlFile.Contains(modlFile) &&
                               !svetModlFile.Contains(modlFile) &&
                               !tgruntModlFile.Contains(modlFile) &&
                               !tvetModlFile.Contains(modlFile) &&
                               !ugruntModlFile.Contains(modlFile) &&
                               !uvetModlFile.Contains(modlFile) &&
                               !wgruntModlFile.Contains(modlFile) &&
                               !wvetModlFile.Contains(modlFile) &&
                               !xgruntModlFile.Contains(modlFile) &&
                               !xvetModlFile.Contains(modlFile)
                       )
                       .ToArray();

          var allModlsAndAnims =
              new (IEnumerable<IFileHierarchyFile>, IList<IReadOnlyTreeFile>?
                  )
                  [] {
                      (sgruntModlFile,
                       fgAnimFiles.Concat(sgAnimFiles).ToArray()),
                      (svetModlFile, fvAnimFiles),
                      (tgruntModlFile, fgAnimFiles),
                      (tvetModlFile, fvAnimFiles),
                      (ugruntModlFile, fgAnimFiles),
                      (uvetModlFile,
                       fvAnimFiles.Concat(uvAnimFiles).ToArray()),
                      (wgruntModlFile,
                       fgAnimFiles.Concat(wgruntAnimFiles).ToArray()),
                      (wvetModlFile, fvAnimFiles),
                      (xgruntModlFile,
                       fgAnimFiles.Concat(xgAnimFiles).ToArray()),
                      (xvetModlFile,
                       fvAnimFiles.Concat(xvAnimFiles).ToArray()),
                      (otherModlFiles, null),
                  };

          foreach (var (currModlFiles, currAnimFiles) in allModlsAndAnims) {
            foreach (var modlFile in currModlFiles) {
              organizer.Add(new ModlModelFileBundle {
                  ModlFile = modlFile,
                  GameVersion = GameVersion.BW1,
                  AnimFiles = currAnimFiles
              }.Annotate(modlFile));
            }
          }

          foreach (var outFile in directory.FilesWithExtension(".out")) {
            organizer.Add(new OutModelFileBundle {
                OutFile = outFile,
                GameVersion = GameVersion.BW1,
            }.Annotate(outFile));
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
                  GameVersion = GameVersion.BW1,
              }.Annotate(levelXmlFile));
            }
          }
        }
    ).GatherFileBundles(organizer, mutablePercentageProgress);
  }
}