using fin.io;
using fin.io.bundles;
using fin.util.progress;

namespace uni.games.doshin_the_giant;

public sealed class DoshinTheGiantFileBundleGatherer : BGameCubeFileBundleGatherer {
  public override string Name => "doshin_the_giant";

  public override bool IsListed => false;

  protected override void GatherFileBundlesFromHierarchy(
      IFileBundleOrganizer organizer,
      IMutablePercentageProgress mutablePercentageProgress,
      IFileHierarchy fileHierarchy) { }
}