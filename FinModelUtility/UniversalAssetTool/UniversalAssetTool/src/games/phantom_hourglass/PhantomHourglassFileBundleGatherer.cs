using fin.io;
using fin.io.bundles;
using fin.util.progress;

namespace uni.games.phantom_hourglass;

public sealed class PhantomHourglassFileBundleGatherer
    : BDsFileBundleGatherer {
  public override string Name => "phantom_hourglass";

  public override bool IsListed => false;

  protected override void GatherFileBundlesFromHierarchy(
      IFileBundleOrganizer organizer,
      IMutablePercentageProgress mutablePercentageProgress,
      IFileHierarchy fileHierarchy) { }
}