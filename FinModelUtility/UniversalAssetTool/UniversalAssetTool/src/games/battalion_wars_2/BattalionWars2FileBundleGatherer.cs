using fin.common;
using fin.io;
using fin.io.bundles;
using fin.util.progress;

using modl.api;

using uni.games.battalion_wars_1;
using uni.platforms.wii;
using uni.util.io;

namespace uni.games.battalion_wars_2;

public class BattalionWars2FileBundleGatherer : IAnnotatedFileBundleGatherer {
  public void GatherFileBundles(
      IFileBundleOrganizer organizer,
      IMutablePercentageProgress mutablePercentageProgress) {
    if (!DirectoryConstants.ROMS_DIRECTORY.TryToGetExistingFile(
            "battalion_wars_2.iso",
            out var battalionWarsRom)) {
      return;
    }

    var fileHierarchy =
        new WiiFileHierarchyExtractor().ExtractFromRom(
            battalionWarsRom);

    var didUpdateAny = false;
    foreach (var directory in fileHierarchy) {
      var didUpdate = false;
      var resFiles =
          directory.GetExistingFiles()
                   .Where(file => file.Name.EndsWith(".res.gz"));
      foreach (var resFile in resFiles) {
        didUpdateAny |= didUpdate |= new ResDump().Run(resFile);
      }

      if (didUpdate) {
        directory.Refresh();
      }
    }

    if (didUpdateAny) {
      fileHierarchy.RefreshRoot();
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
              animFiles.Where(
                           animFile =>
                               animFile.NameWithoutExtension.StartsWith("FV"))
                       .ToArray();
          var wgruntAnimFiles =
              animFiles.Where(
                           animFile =>
                               animFile.NameWithoutExtension.StartsWith(
                                   "WG"))
                       .ToArray();

          var otherModlFiles =
              modlFiles.Where(
                           modlFile =>
                               !svetModlFile.Contains(modlFile) &&
                               !gruntModlFiles.Contains(modlFile) &&
                               !vetModlFiles.Contains(modlFile)
                       )
                       .ToArray();

          var allModlsAndAnims =
              new (IEnumerable<IFileHierarchyFile>, IList<IReadOnlyTreeFile>?
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
                  GameName = "battalion_wars_2",
                  ModlFile = modlFile,
                  GameVersion = GameVersion.BW2,
                  AnimFiles = currAnimFiles
              }.Annotate(modlFile));
            }
          }

          foreach (var outFile in directory.GetExistingFiles()
                                           .Where(file => file.Name.EndsWith(
                                                      ".out.gz"))) {
            organizer.Add(new OutModelFileBundle {
                GameName = "battalion_wars_2",
                OutFile = outFile,
                GameVersion = GameVersion.BW2,
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
                  GameName = "battalion_wars_2",
                  MainXmlFile = levelXmlFile,
                  GameVersion = GameVersion.BW2,
              }.Annotate(levelXmlFile));
            }
          }
        }
    ).GatherFileBundles(organizer, mutablePercentageProgress);
  }
}