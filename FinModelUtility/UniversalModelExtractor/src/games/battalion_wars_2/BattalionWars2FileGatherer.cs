using fin.io;
using fin.io.bundles;
using modl.api;
using uni.games.battalion_wars_1;
using uni.platforms;
using uni.platforms.wii;
using uni.util.io;


namespace uni.games.battalion_wars_2 {
  public class BattalionWars2FileGatherer : IFileBundleGatherer<IFileBundle> {
    public IEnumerable<IFileBundle> GatherFileBundles(bool assert) {
      if (!DirectoryConstants.ROMS_DIRECTORY.PossiblyAssertExistingFile(
              "battalion_wars_2.iso",
              assert, out var battalionWarsRom)) {
        return Enumerable.Empty<IFileBundle>();
      }

      var fileHierarchy =
          new WiiFileHierarchyExtractor().ExtractFromRom(
              battalionWarsRom);

      foreach (var directory in fileHierarchy) {
        var didUpdate = false;
        var resFiles =
            directory.Files.Where(file => file.Name.EndsWith(".res.gz"));
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
                new (IEnumerable<IFileHierarchyFile>, IList<IFileHierarchyFile>?
                    )
                    [] {
                        (svetModlFile, fvAnimFiles),
                        (gruntModlFiles, wgruntAnimFiles),
                        (vetModlFiles, fvAnimFiles), (otherModlFiles, null),
                    };

            var modlBundles =
                allModlsAndAnims
                    .SelectMany(
                        modlsAndAnims =>
                            modlsAndAnims
                                .Item1
                                .Select(
                                    modlFile => new ModlModelFileBundle {
                                        GameName = "battalion_wars_2",
                                        ModlFile = modlFile,
                                        GameVersion = GameVersion.BW2,
                                        AnimFiles = modlsAndAnims.Item2
                                    }));
            var outBundles =
                directory.Files
                         .Where(file => file.Name.EndsWith(".out.gz"))
                         .Select(outFile => new OutModelFileBundle {
                             GameName = "battalion_wars_2",
                             OutFile = outFile, 
                             GameVersion = GameVersion.BW2,
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
                          GameName = "battalion_wars_2",
                          MainXmlFile = file,
                          GameVersion = GameVersion.BW2,
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