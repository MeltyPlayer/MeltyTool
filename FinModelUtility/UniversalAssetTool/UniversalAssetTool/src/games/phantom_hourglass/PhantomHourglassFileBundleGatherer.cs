using fin.io;
using fin.io.bundles;
using fin.util.progress;

namespace uni.games.phantom_hourglass;

public class PhantomHourglassFileBundleGatherer
    : BDsFileBundleGatherer {
  public override string Name => "phantom_hourglass";

  protected override void GatherFileBundlesFromHierarchy(
      IFileBundleOrganizer organizer,
      IMutablePercentageProgress mutablePercentageProgress,
      IFileHierarchy fileHierarchy) { }
}