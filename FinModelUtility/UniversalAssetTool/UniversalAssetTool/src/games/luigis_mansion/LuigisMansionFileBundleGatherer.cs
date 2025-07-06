using fin.io;
using fin.io.bundles;
using fin.util.progress;

using uni.platforms.gcn;

namespace uni.games.luigis_mansion;

public class LuigisMansionFileBundleGatherer : BGameCubeFileBundleGatherer {
  public override string Name => "luigis_mansion";

  public override GcnFileHierarchyExtractor.Options Options
    => GcnFileHierarchyExtractor
       .Options.Standard()
       .UseRarcDumpForExtensions(
           // For some reason, some MDL files are compressed as RARC.
           ".mdl");

  protected override void GatherFileBundlesFromHierarchy(
      IFileBundleOrganizer organizer,
      IMutablePercentageProgress mutablePercentageProgress,
      IFileHierarchy fileHierarchy) { }
}