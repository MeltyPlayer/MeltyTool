using dat.api;
using fin.io.bundles;

using uni.platforms;
using uni.platforms.gcn;


namespace uni.games.super_smash_bros_melee {
  public class SuperSmashBrosMeleeModelFileGatherer 
      : IFileBundleGatherer<DatModelFileBundle> {
    public string Name => "super_smash_bros_melee";

    public IEnumerable<DatModelFileBundle>? GatherFileBundles(bool assert) {
      if (!DirectoryConstants.ROMS_DIRECTORY.PossiblyAssertExistingFile(
              "super_smash_bros_melee.gcm",
              assert,
              out var superSmashBrosMeleeRom)) {
        yield break;
      }

      var options = GcnFileHierarchyExtractor.Options.Standard();
      var fileHierarchy =
          new GcnFileHierarchyExtractor()
              .ExtractFromRom(options, superSmashBrosMeleeRom);

      foreach (var datFile in fileHierarchy.Root.FilesWithExtension(".dat")) {
        var datFileName = datFile.NameWithoutExtension;

        var isValidModel = false;

        // Playable character data
        if (datFileName.StartsWith("Pl")) {
          if (datFileName.EndsWith("Nr")) {
            isValidModel = true;
          }
        }

        // Stages
        if (datFileName.StartsWith("Gr")) {
          isValidModel = true;
        }

        // Trophies
        if (datFileName.StartsWith("Ty")) {
          isValidModel = true;
        }

        // TODO: Look into the other files

        if (isValidModel) {
          yield return new DatModelFileBundle {
              GameName = "super_smash_bros_melee",
              DatFile = datFile,
          };
        }
      }
    }
  }
}