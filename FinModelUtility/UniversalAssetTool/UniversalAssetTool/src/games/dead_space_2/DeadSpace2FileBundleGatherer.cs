using fin.io.bundles;
using fin.util.progress;

using uni.platforms.desktop;

namespace uni.games.dead_space_2;

public sealed class DeadSpace2FileBundleGatherer : INamedFileBundleGatherer {
  public string Name => "dead_space_2";

  public bool IsListed => false;
  public bool IsAvailable
    => SteamUtils.TryGetGameDirectory("Dead Space 2", out _) ||
       EaUtils.TryGetGameDirectory("Dead Space 2", out _);

  public void GatherFileBundles(
      IFileBundleOrganizer organizer,
      IMutablePercentageProgress mutablePercentageProgress) {
    if (!EaUtils.TryGetGameDirectory("Dead Space 2", out var deadSpace2Dir)) {
      return;
    }

    var originalGameFileHierarchy
        = ExtractorUtil.GetFileHierarchy("dead_space_2", deadSpace2Dir);
  }
}