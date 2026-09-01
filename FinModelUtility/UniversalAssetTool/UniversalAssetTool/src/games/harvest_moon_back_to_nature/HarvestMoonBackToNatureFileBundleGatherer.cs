using fin.archives;
using fin.io;
using fin.io.bundles;
using fin.util.progress;

namespace uni.games.harvest_moon_back_to_nature;

public sealed class HarvestMoonBackToNatureFileBundleGatherer : BPs1FileBundleGatherer {
  public override string Name => "harvest_moon_back_to_nature";

  protected override void GatherFileBundlesFromHierarchy(
      IFileBundleOrganizer organizer,
      IMutablePercentageProgress mutablePercentageProgress,
      IFileHierarchy fileHierarchy) {
  }
}