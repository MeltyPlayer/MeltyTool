using grezzo.api;

using fin.io;
using fin.io.bundles;

using uni.platforms.threeDs;
using uni.util.bundles;

namespace uni.games.luigis_mansion_3d;

using IAnnotatedCmbBundle = IAnnotatedFileBundle<CmbModelFileBundle>;

public class LuigisMansion3dFileBundleGatherer
    : IAnnotatedFileBundleGatherer<CmbModelFileBundle> {
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

  public IEnumerable<IAnnotatedCmbBundle> GatherFileBundles() {
      if (!new ThreeDsFileHierarchyExtractor().TryToExtractFromGame(
              "luigis_mansion_3d",
              out var fileHierarchy)) {
        return Enumerable.Empty<IAnnotatedCmbBundle>();
      }

      return fileHierarchy.SelectMany(this.ExtractModel_);
    }

  private IEnumerable<IAnnotatedCmbBundle> ExtractModel_(
      IFileHierarchyDirectory subdir) {
      var cmbFiles = subdir.FilesWithExtension(".cmb").ToArray();
      if (cmbFiles.Length == 0) {
        return Enumerable.Empty<IAnnotatedCmbBundle>();
      }

      var csabFiles = subdir.FilesWithExtension(".csab").ToArray();
      var ctxbFiles = subdir.FilesWithExtension(".ctxb").ToArray();
      var shpaFiles = subdir.FilesWithExtension(".shpa").ToArray();

      try {
        return this.separator_.Separate(subdir, cmbFiles, csabFiles)
                   .Select(bundle => new CmbModelFileBundle(
                               "luigis_mansion_3d",
                               bundle.ModelFile,
                               bundle.AnimationFiles.ToArray(),
                               ctxbFiles,
                               shpaFiles
                           ).Annotate(bundle.ModelFile));
      } catch {
        return Enumerable.Empty<IAnnotatedCmbBundle>();
      }
    }
}