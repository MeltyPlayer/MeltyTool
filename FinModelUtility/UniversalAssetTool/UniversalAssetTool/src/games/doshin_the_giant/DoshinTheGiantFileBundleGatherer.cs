using fin.io;
using fin.io.bundles;
using fin.util.progress;

namespace uni.games.doshin_the_giant;

public class DoshinTheGiantFileBundleGatherer : BGameCubeFileBundleGatherer {
  public override string Name => "doshin_the_giant";

  protected override void GatherFileBundlesFromHierarchy(
      IFileBundleOrganizer organizer,
      IMutablePercentageProgress mutablePercentageProgress,
      IFileHierarchy fileHierarchy) { }
}