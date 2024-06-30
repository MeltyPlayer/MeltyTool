using fin.io;
using fin.io.bundles;
using fin.util.progress;

using uni.platforms.desktop;

namespace uni.games.dead_space_3 {
  public class DeadSpace3FileBundleGatherer : IAnnotatedFileBundleGatherer {
    public void GatherFileBundles(
        IFileBundleOrganizer organizer,
        IMutablePercentageProgress mutablePercentageProgress) {
      if (!EaUtils.TryGetGameDirectory("Dead Space 3", out var deadSpace3Dir)) {
        return;
      }

      var originalGameFileHierarchy
          = FileHierarchy.From("dead_space_3", deadSpace3Dir);
    }
  }
}