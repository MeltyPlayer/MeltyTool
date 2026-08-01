using fin.io;
using fin.io.bundles;
using fin.util.progress;

namespace uni.games.mario_kart_ds;

public sealed class MarioKartDsFileBundleGatherer : BDsFileBundleGatherer {
  public override string Name => "mario_kart_ds";

  public override bool IsListed => false;

  protected override void GatherFileBundlesFromHierarchy(
      IFileBundleOrganizer organizer,
      IMutablePercentageProgress mutablePercentageProgress,
      IFileHierarchy fileHierarchy) { }
}