using fin.io;
using fin.io.bundles;
using fin.util.progress;

using pikmin1.api;

using uni.platforms.gcn;
using uni.util.bundles;
using uni.util.io;

namespace uni.games.pikmin_1;

public class Pikmin1FileBundleGatherer : BGameCubeFileBundleGatherer {
  private readonly IModelSeparator separator_
      = new ModelSeparator(directory => directory.LocalPath)
          .Register(new AllAnimationsModelSeparatorMethod(),
                    @"\dataDir\pikis");

  public override string Name => "pikmin_1";

  public override GcnFileHierarchyExtractor.Options Options
    => GcnFileHierarchyExtractor.Options.Empty();

  protected override void GatherFileBundlesFromHierarchy(
      IFileBundleOrganizer organizer,
      IMutablePercentageProgress mutablePercentageProgress,
      IFileHierarchy fileHierarchy) {
    new AnnotatedFileBundleGathererAccumulatorWithInput<IFileHierarchy>(
            fileHierarchy)
        .Add(this.GetAutomaticModels_)
        .Add(this.GetModelsViaSeparator_)
        .GatherFileBundles(organizer, mutablePercentageProgress);
  }

  private void GetAutomaticModels_(
      IFileBundleOrganizer organizer,
      IFileHierarchy fileHierarchy) {
    foreach (var directory in fileHierarchy) {
      if (this.separator_.Contains(directory)) {
        continue;
      }

      var anmFiles = directory.FilesWithExtension(".anm").ToArray();
      foreach (var modFile in directory.FilesWithExtension(".mod")) {
        var anmFile = anmFiles.FirstOrDefault(
            anmFile => anmFile.NameWithoutExtension.SequenceEqual(
                modFile.NameWithoutExtension));
        organizer.Add(new ModModelFileBundle {
            ModFile = modFile,
            AnmFile = anmFile,
        }.Annotate(modFile));
      }
    }
  }

  private void GetModelsViaSeparator_(
      IFileBundleOrganizer organizer,
      IMutablePercentageProgress progress,
      IFileHierarchy fileHierarchy)
    => new FileHierarchyAssetBundleSeparator(
        fileHierarchy,
        (subdir, organizer) => {
          if (!this.separator_.Contains(subdir)) {
            return;
          }

          var modFiles =
              subdir.FilesWithExtensions(".mod").ToArray();
          if (modFiles.Length == 0) {
            return;
          }

          var anmFiles =
              subdir.FilesWithExtensions(".anm").ToArray();

          try {
            foreach (var bundle in this.separator_.Separate(
                         subdir,
                         modFiles,
                         anmFiles)) {
              organizer.Add(new ModModelFileBundle {
                  ModFile = bundle.ModelFile,
                  AnmFile = bundle.AnimationFiles.SingleOrDefault(),
              }.Annotate(bundle.ModelFile));
            }
          } catch { }
        }
    ).GatherFileBundles(organizer, progress);
}