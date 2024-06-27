using fin.io;
using fin.io.bundles;

using mod.api;

using uni.platforms.gcn;
using uni.util.bundles;
using uni.util.io;

namespace uni.games.pikmin_1 {
  using IAnnotatedModBundle = IAnnotatedFileBundle<ModModelFileBundle>;

  public class Pikmin1FileBundleGatherer
      : IAnnotatedFileBundleGatherer<ModModelFileBundle> {
    private readonly IModelSeparator separator_
        = new ModelSeparator(directory => directory.LocalPath)
            .Register(new AllAnimationsModelSeparatorMethod(),
                      @"\dataDir\pikis");

    public IEnumerable<IAnnotatedModBundle> GatherFileBundles() {
      if (!new GcnFileHierarchyExtractor().TryToExtractFromGame(
              "pikmin_1",
              GcnFileHierarchyExtractor.Options.Empty(),
              out var fileHierarchy)) {
        return Enumerable.Empty<IAnnotatedModBundle>();
      }

      return new AnnotatedFileBundleGathererAccumulatorWithInput<
                 ModModelFileBundle,
                 IFileHierarchy>(
                 fileHierarchy)
             .Add(this.GetAutomaticModels_)
             .Add(this.GetModelsViaSeparator_)
             .GatherFileBundles();
    }

    private IEnumerable<IAnnotatedModBundle> GetAutomaticModels_(
        IFileHierarchy fileHierarchy) {
      return fileHierarchy.SelectMany(directory => {
        if (this.separator_.Contains(directory)) {
          return Enumerable.Empty<IAnnotatedModBundle>();
        }

        var anmFiles = directory.FilesWithExtension(".anm").ToArray();
        return directory
               .FilesWithExtension(".mod")
               .Select(modFile => {
                 var anmFile = anmFiles
                     .FirstOrDefault(
                         anmFile => anmFile.NameWithoutExtension ==
                                    modFile.NameWithoutExtension);
                 return new ModModelFileBundle {
                     GameName = "pikmin_1",
                     ModFile = modFile,
                     AnmFile = anmFile,
                 }.Annotate(modFile);
               });
      });
    }

    private IEnumerable<IAnnotatedModBundle> GetModelsViaSeparator_(
        IFileHierarchy fileHierarchy)
      => new FileHierarchyAssetBundleSeparator<ModModelFileBundle>(
          fileHierarchy,
          subdir => {
            if (!this.separator_.Contains(subdir)) {
              return Enumerable.Empty<IAnnotatedModBundle>();
            }

            var modFiles =
                subdir.FilesWithExtensions(".mod").ToArray();
            if (modFiles.Length == 0) {
              return Enumerable.Empty<IAnnotatedModBundle>();
            }

            var anmFiles =
                subdir.FilesWithExtensions(".anm").ToArray();

            try {
              var bundles =
                  this.separator_.Separate(subdir, modFiles, anmFiles);

              return bundles.Select(bundle => new ModModelFileBundle {
                  GameName = "pikmin_1",
                  ModFile = bundle.ModelFile,
                  AnmFile = bundle.AnimationFiles.SingleOrDefault(),
              }.Annotate(bundle.ModelFile));
            } catch {
              return Enumerable.Empty<IAnnotatedModBundle>();
            }
          }
      ).GatherFileBundles();
  }
}