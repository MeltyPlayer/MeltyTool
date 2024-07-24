using fin.io;
using fin.io.bundles;
using fin.util.progress;

using uni.platforms.desktop;

namespace uni.games.dead_space_2;

public class DeadSpace2FileBundleGatherer : IAnnotatedFileBundleGatherer {
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