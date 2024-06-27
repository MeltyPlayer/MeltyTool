using uni.platforms.gcn;

namespace uni.games.luigis_mansion;

public class LuigisMansionMassExporter : IMassExporter {
  public void ExportAll() {
      if (!new GcnFileHierarchyExtractor().TryToExtractFromGame(
              "luigis_mansion",
              GcnFileHierarchyExtractor.Options
                                       .Standard()
                                       .UseRarcDumpForExtensions(
                                           // For some reason, some MDL files are compressed as RARC.
                                           ".mdl"),
              out var fileHierarchy)) {
        return;
      }

      // TODO: Use Mdl2Fbx
    }
}