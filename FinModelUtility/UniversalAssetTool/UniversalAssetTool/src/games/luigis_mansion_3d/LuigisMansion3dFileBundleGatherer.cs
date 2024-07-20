using grezzo.api;

using fin.io.bundles;

using uni.platforms.threeDs;
using uni.util.bundles;

using fin.util.progress;

namespace uni.games.luigis_mansion_3d;

public class LuigisMansion3dFileBundleGatherer
    : IAnnotatedFileBundleGatherer {
  private readonly IModelSeparator separator_ =
      new ModelSeparator(directory => directory.LocalPath)
          .Register(@"\effect\effect_mdl", new PrefixModelSeparatorMethod())
          .Register(@"\model\dluige01",
                    new NameModelSeparatorMethod("Luigi.cmb"))
          .Register(@"\model\dluige02",
                    new NameModelSeparatorMethod("Luigi.cmb"))
          .Register(@"\model\dluige03",
                    new NameModelSeparatorMethod("Luigi.cmb"))
          .Register(@"\model\luige",
                    new NameModelSeparatorMethod("Luigi.cmb"));

  public void GatherFileBundles(
      IFileBundleOrganizer organizer,
      IMutablePercentageProgress mutablePercentageProgress) {
    if (!new ThreeDsFileHierarchyExtractor().TryToExtractFromGame(
            "luigis_mansion_3d",
            out var fileHierarchy)) {
      return;
    }

    foreach (var subdir in fileHierarchy) {
      var cmbFiles = subdir.FilesWithExtension(".cmb").ToArray();
      if (cmbFiles.Length == 0) {
        continue;
      }

      var csabFiles = subdir.FilesWithExtension(".csab").ToArray();
      var ctxbFiles = subdir.FilesWithExtension(".ctxb").ToArray();
      var shpaFiles = subdir.FilesWithExtension(".shpa").ToArray();

      try {
        foreach (var bundle in this.separator_.Separate(
                     subdir,
                     cmbFiles,
                     csabFiles)) {
          organizer.Add(new CmbModelFileBundle(
                            "luigis_mansion_3d",
                            bundle.ModelFile,
                            bundle.AnimationFiles.ToArray(),
                            ctxbFiles,
                            shpaFiles
                        ).Annotate(bundle.ModelFile));
        }
      } catch { }
    }
  }
}