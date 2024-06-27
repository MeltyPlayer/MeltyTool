using fin.io;
using fin.io.bundles;

using uni.platforms.desktop;

namespace uni.games.dead_space_3;

public class DeadSpace3FileBundleGatherer : IAnnotatedFileBundleGatherer {
  public IEnumerable<IAnnotatedFileBundle> GatherFileBundles() {
      if (!EaUtils.TryGetGameDirectory("Dead Space 3", out var deadSpace3Dir)) {
        yield break;
      }

      var originalGameFileHierarchy
          = FileHierarchy.From("dead_space_3", deadSpace3Dir);
    }
}