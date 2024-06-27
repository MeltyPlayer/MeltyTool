using fin.io.bundles;
using fin.model.io;

using uni.platforms.gcn;

namespace uni.games.luigis_mansion;

using IAnnotatedMdlBundle = IAnnotatedFileBundle<IModelFileBundle>;

public class LuigisMansionFileBundleGatherer
    : IAnnotatedFileBundleGatherer<IModelFileBundle> {
  public IEnumerable<IAnnotatedMdlBundle> GatherFileBundles() {
      if (!new GcnFileHierarchyExtractor().TryToExtractFromGame(
              "luigis_mansion",
              GcnFileHierarchyExtractor.Options
                                       .Standard()
                                       .UseRarcDumpForExtensions(
                                           // For some reason, some MDL files are compressed as RARC.
                                           ".mdl"),
              out var fileHierarchy)) {
        yield break;
      }
    }
}