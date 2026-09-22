using fin.io;
using fin.io.bundles;
using fin.util.progress;

namespace uni.games.soulcalibur_ii;

public sealed class SoulcaliburIIFileBundleGatherer : BGameCubeFileBundleGatherer {
  public override string Name => "soulcalibur_ii";
  public override string Title => "Soulcalibur II";

  public override bool IsListed => false;

  protected override void GatherFileBundlesFromHierarchy(
      IFileBundleOrganizer organizer,
      IMutablePercentageProgress mutablePercentageProgress,
      IFileHierarchy fileHierarchy) { }
}