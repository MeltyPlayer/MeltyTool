using dat.api;

using fin.io;
using fin.io.bundles;
using fin.util.progress;

using uni.platforms.gcn;

namespace uni.games.super_smash_bros_melee {
  using IAnnotatedDatBundle = IAnnotatedFileBundle<DatModelFileBundle>;

  public class SuperSmashBrosMeleeFileBundleGatherer
      : IAnnotatedFileBundleGatherer<DatModelFileBundle> {
    public string Name => "super_smash_bros_melee";

    private const string STAGE_PREFIX = "Gr";
    private const string TROPHY_PREFIX = "Ty";

    private const string CHARACTER_PREFIX = "Pl";
    private const string ANIMATION_SUFFIX = "AJ";

    public IEnumerable<IAnnotatedDatBundle>? GatherFileBundles(
        IMutablePercentageProgress mutablePercentageProgress) {
      if (!new GcnFileHierarchyExtractor().TryToExtractFromGame(
              "super_smash_bros_melee",
              out var fileHierarchy)) {
        yield break;
      }

      var stageFiles = new LinkedList<IFileHierarchyFile>();
      var trophyFiles = new LinkedList<IFileHierarchyFile>();
      var plFilesByNameWithoutExtension =
          new Dictionary<string, IFileHierarchyFile>();

      foreach (var datFile in fileHierarchy.Root.FilesWithExtension(".dat")) {
        var datNameWithoutExtension = datFile.NameWithoutExtension;

        if (datNameWithoutExtension.StartsWith(STAGE_PREFIX)) {
          stageFiles.AddLast(datFile);
          continue;
        }

        if (datNameWithoutExtension.StartsWith(TROPHY_PREFIX)) {
          trophyFiles.AddLast(datFile);
          continue;
        }

        if (datNameWithoutExtension.StartsWith(CHARACTER_PREFIX)) {
          plFilesByNameWithoutExtension.Add(datNameWithoutExtension, datFile);
        }
      }

      foreach (var stageOrTrophyFile in stageFiles.Concat(trophyFiles)) {
        yield return new DatModelFileBundle {
            GameName = "super_smash_bros_melee",
            PrimaryDatFile = stageOrTrophyFile,
        }.Annotate(stageOrTrophyFile);
      }

      // TODO: How to optimize this??
      foreach (var (plNameWithoutExtension, plFile) in
               plFilesByNameWithoutExtension) {
        if (!plFilesByNameWithoutExtension.TryGetValue(
                $"{plNameWithoutExtension}{ANIMATION_SUFFIX}",
                out var animationFile)) {
          continue;
        }

        var fighterFile = plFile;

        var plFilesStartingWithName =
            plFilesByNameWithoutExtension
                .Values
                .Where(otherPlFile => {
                  var otherPlNameWithoutExtension =
                      otherPlFile.NameWithoutExtension;
                  return otherPlNameWithoutExtension.StartsWith(
                             plNameWithoutExtension) &&
                         otherPlNameWithoutExtension.Length >
                         plNameWithoutExtension.Length;
                })
                .ToDictionary(file => file.NameWithoutExtension);

        foreach (var modelFile in
                 plFilesStartingWithName
                     .Where(pair => !pair.Key.EndsWith(ANIMATION_SUFFIX))
                     .Select(pair => pair.Value)) {
          yield return new DatModelFileBundle {
              GameName = "super_smash_bros_melee",
              PrimaryDatFile = modelFile,
              AnimationDatFile = animationFile,
              FighterDatFile = fighterFile,
          }.Annotate(modelFile);
        }
      }
    }
  }
}