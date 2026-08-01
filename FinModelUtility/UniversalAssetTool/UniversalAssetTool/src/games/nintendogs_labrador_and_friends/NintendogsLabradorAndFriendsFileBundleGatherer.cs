using fin.io;
using fin.io.bundles;
using fin.util.progress;

namespace uni.games.nintendogs_labrador_and_friends;

public sealed class NintendogsLabradorAndFriendsFileBundleGatherer
    : BDsFileBundleGatherer {
  public override string Name => "nintendogs_labrador_and_friends";

  public override bool IsListed => false;

  protected override void GatherFileBundlesFromHierarchy(
      IFileBundleOrganizer organizer,
      IMutablePercentageProgress mutablePercentageProgress,
      IFileHierarchy fileHierarchy) { }
}