﻿using fin.io;
using fin.io.bundles;
using modl.api;
using uni.platforms;
using uni.platforms.gcn;
using uni.util.io;


namespace uni.games.battalion_wars_1 {
  public class BattalionWars1FileGatherer : IFileBundleGatherer<IFileBundle> {
    public IEnumerable<IFileBundle> GatherFileBundles(bool assert) {
      if (!DirectoryConstants.ROMS_DIRECTORY.PossiblyAssertExistingFile(
              "battalion_wars_1.gcm",
              assert,
              out var battalionWarsRom)) {
        return Enumerable.Empty<IFileBundle>();
      }

      var options = GcnFileHierarchyExtractor.Options.Standard();

      var fileHierarchy =
          new GcnFileHierarchyExtractor().ExtractFromRom(
              options,
              battalionWarsRom);

      foreach (var directory in fileHierarchy) {
        var didUpdate = false;
        var resFiles = directory.FilesWithExtension(".res");
        foreach (var resFile in resFiles) {
          didUpdate |= new ResDump().Run(resFile);
        }

        if (didUpdate) {
          directory.Refresh();
        }
      }

      return new FileHierarchyAssetBundleSeparator<IFileBundle>(
          fileHierarchy,
          directory => {
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
                new (IEnumerable<IFileHierarchyFile>, IList<IFileHierarchyFile>?
                    )
                    [] {
                        (sgruntModlFile,
                         fgAnimFiles.Concat(sgAnimFiles).ToArray()),
                        (svetModlFile, fvAnimFiles),
                        (tgruntModlFile, fgAnimFiles),
                        (tvetModlFile, fvAnimFiles),
                        (ugruntModlFile, fgAnimFiles), (uvetModlFile,
                              fvAnimFiles.Concat(uvAnimFiles).ToArray()),
                        (wgruntModlFile,
                         fgAnimFiles.Concat(wgruntAnimFiles).ToArray()),
                        (wvetModlFile, fvAnimFiles), (xgruntModlFile,
                              fgAnimFiles.Concat(xgAnimFiles).ToArray()),
                        (xvetModlFile,
                         fvAnimFiles.Concat(xvAnimFiles).ToArray()),
                        (otherModlFiles, null),
                    };

            var modlBundles =
                allModlsAndAnims
                    .SelectMany(
                        modlsAndAnims =>
                            modlsAndAnims
                                .Item1
                                .Select(
                                    modlFile => new ModlModelFileBundle {
                                        GameName = "battalion_wars_1",
                                        ModlFile = modlFile,
                                        GameVersion = GameVersion.BW1,
                                        AnimFiles = modlsAndAnims.Item2
                                    }))
                    .ToList();
            var outBundles =
                directory.FilesWithExtension(".out")
                         .Select(outFile => new OutModelFileBundle {
                             GameName = "battalion_wars_1",
                             OutFile = outFile, 
                             GameVersion = GameVersion.BW1,
                         });
            var sceneBundles =
                directory.Name == "CompoundFiles"
                    ? directory
                      .FilesWithExtension(".xml")
                      .Where(file =>
                                 !file.NameWithoutExtension.EndsWith("_Level"))
                      .Where(file =>
                                 !file.NameWithoutExtension.EndsWith("_preload"))
                      .Select(file => new BwSceneFileBundle {
                          GameName = "battalion_wars_1",
                          MainXmlFile = file,
                          GameVersion = GameVersion.BW1,
                      })
                    : new List<BwSceneFileBundle>();

            var bundles =
                modlBundles.Concat<IBattalionWarsFileBundle>(outBundles)
                           .Concat(sceneBundles)
                           .ToList();
            bundles.Sort((lhs, rhs) =>
                             lhs.MainFile.Name.CompareTo(
                                 rhs.MainFile.Name));

            return bundles;
          }
      ).GatherFileBundles(assert);
    }
  }
}