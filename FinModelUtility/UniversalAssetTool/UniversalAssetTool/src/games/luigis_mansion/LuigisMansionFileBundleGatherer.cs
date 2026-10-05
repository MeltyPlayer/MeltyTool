using fin.io;
using fin.io.bundles;
using fin.util.progress;

using uni.platforms.gcn;

namespace uni.games.luigis_mansion;

// https://wiki.dolphin-emu.org/index.php?title=GLME01
[GameIDs("GLME01", "GLMJ01", "GLMK01", "GLMP01")]
public sealed class LuigisMansionFileBundleGatherer : BGameCubeFileBundleGatherer {
  public override string Name => "luigis_mansion";
  public override string Title => "Luigi's Mansion";

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