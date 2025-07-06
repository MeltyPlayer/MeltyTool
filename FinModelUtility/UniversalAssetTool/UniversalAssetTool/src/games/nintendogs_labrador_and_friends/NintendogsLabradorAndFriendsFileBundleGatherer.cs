using fin.io;
using fin.io.bundles;
using fin.util.progress;

namespace uni.games.nintendogs_labrador_and_friends;

public class NintendogsLabradorAndFriendsFileBundleGatherer
    : BDsFileBundleGatherer {
  public override string Name => "nintendogs_labrador_and_friends";

  protected override void GatherFileBundlesFromHierarchy(
      IFileBundleOrganizer organizer,
      IMutablePercentageProgress mutablePercentageProgress,
      IFileHierarchy fileHierarchy) { }
}