using sysdolphin.api;

using fin.io;
using fin.io.bundles;
using fin.util.progress;

using ssm.api;

namespace uni.games.super_smash_bros_melee;

public class SuperSmashBrosMeleeFileBundleGatherer
    : BGameCubeFileBundleGatherer {
  private const string STAGE_PREFIX = "Gr";
  private const string TROPHY_PREFIX = "Ty";

  private const string CHARACTER_PREFIX = "Pl";
  private const string ANIMATION_SUFFIX = "AJ";

  public override string Name => "super_smash_bros_melee";

  protected override void GatherFileBundlesFromHierarchy(
      IFileBundleOrganizer organizer,
      IMutablePercentageProgress mutablePercentageProgress,
      IFileHierarchy fileHierarchy) {
    var stageFiles = new LinkedList<IFileHierarchyFile>();
    var trophyFiles = new LinkedList<IFileHierarchyFile>();
    var plFilesByNameWithoutExtension =
        new Dictionary<string, IFileHierarchyFile>();

    foreach (var ssmFile in
             fileHierarchy.Root.FilesWithExtensionRecursive(".ssm")) {
      organizer.Add(new SsmAudioFileBundle {
         SsmFile = ssmFile,
      }.Annotate(ssmFile));
    }

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
        plFilesByNameWithoutExtension.Add(datNameWithoutExtension.ToString(),
                                          datFile);
      }
    }

    foreach (var stageOrTrophyFile in stageFiles.Concat(trophyFiles)) {
      organizer.Add(new DatModelFileBundle {
          DatFile = stageOrTrophyFile,
      }.Annotate(stageOrTrophyFile));
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
              .ToDictionary(file => file.NameWithoutExtension.ToString());

      foreach (var modelFile in
               plFilesStartingWithName
                   .Where(pair => !pair.Key.EndsWith(ANIMATION_SUFFIX))
                   .Select(pair => pair.Value)) {
        organizer.Add(new MeleeModelFileBundle {
            PrimaryDatFile = modelFile,
            AnimationDatFile = animationFile,
            FighterDatFile = fighterFile,
        }.Annotate(modelFile));
      }
    }
  }
}