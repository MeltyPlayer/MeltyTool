using fin.io;
using fin.io.bundles;
using fin.util.progress;

namespace uni.games.great_ace_attorney;

public sealed class GreatAceAttorneyFileBundleGatherer
    : B3dsFileBundleGatherer {
  public override string Name => "great_ace_attorney";

  public override bool IsListed => false;

  protected override void GatherFileBundlesFromHierarchy(
      IFileBundleOrganizer organizer,
      IMutablePercentageProgress mutablePercentageProgress,
      IFileHierarchy fileHierarchy) { }
}