using fin.data.sets;
using fin.io;
using fin.io.bundles;
using fin.util.progress;

using modl.api;

using uni.util.io;

namespace uni.games.battalion_wars_1;

public class BattalionWars1FileBundleGatherer : BGameCubeFileBundleGatherer {
  public override string Name => "battalion_wars_1";

  protected override void GatherFileBundlesFromHierarchy(
      IFileBundleOrganizer organizer,
      IMutablePercentageProgress mutablePercentageProgress,
      IFileHierarchy fileHierarchy) {
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
          var modlSplitter
              = directory.FilesWithExtension(".modl").SplitByName();

          var svetModlFile = modlSplitter.StartsWith("SVET.modl");
          var sgruntModlFile = modlSplitter.StartsWith("SGRUNT.modl");

          var tvetModlFile = modlSplitter.StartsWith("TVET.modl");
          var tgruntModlFile = modlSplitter.StartsWith("TGRUNT.modl");

          var uvetModlFile = modlSplitter.StartsWith("UVET.modl");
          var ugruntModlFile = modlSplitter.StartsWith("UGRUNT.modl");

          var wvetModlFile = modlSplitter.StartsWith("WVET.modl");
          var wgruntModlFile = modlSplitter.StartsWith("WGRUNT.modl");

          var xvetModlFile = modlSplitter.StartsWith("XVET.modl");
          var xgruntModlFile = modlSplitter.StartsWith("XGRUNT.modl");

          var animSplitter
              = directory.FilesWithExtension(".anim").SplitByName();

          var fvAnimFiles = animSplitter.StartsWith("FV");
          var fgAnimFiles = animSplitter.StartsWith("FG");
          
          var sgAnimFiles = animSplitter.StartsWith("SG");
          var uvAnimFiles = animSplitter.StartsWith("UV");

          var wgruntAnimFiles = animSplitter.StartsWith("WGRUNT");
          var xgAnimFiles = animSplitter.StartsWith("XG");
          var xvAnimFiles = animSplitter.StartsWith("XV");

          var allModlsAndAnims =
              new (IEnumerable<IFileHierarchyFile>, IReadOnlyList<IReadOnlyTreeFile>?
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
                      (modlSplitter.Remaining(), null),
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