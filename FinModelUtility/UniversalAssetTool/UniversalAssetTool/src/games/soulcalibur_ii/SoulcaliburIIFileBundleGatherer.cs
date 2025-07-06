using fin.io;
using fin.io.bundles;
using fin.util.progress;

namespace uni.games.soulcalibur_ii;

public class SoulcaliburIIFileBundleGatherer : BGameCubeFileBundleGatherer {
  public override string Name => "soulcalibur_ii";

  protected override void GatherFileBundlesFromHierarchy(
      IFileBundleOrganizer organizer,
      IMutablePercentageProgress mutablePercentageProgress,
      IFileHierarchy fileHierarchy) { }
}