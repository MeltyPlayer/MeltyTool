using fin.io;
using fin.io.bundles;
using fin.util.progress;

using uni.platforms.gcn;

namespace uni.games.nddemo;

public sealed class NddemoFileBundleGatherer : BGameCubeFileBundleGatherer {
  public override string Name => "nddemo";

  public override bool IsListed => false;

  public override GcnFileHierarchyExtractor.Options Options
    => GcnFileHierarchyExtractor.Options.Standard();

  protected override void GatherFileBundlesFromHierarchy(
      IFileBundleOrganizer organizer,
      IMutablePercentageProgress mutablePercentageProgress,
      IFileHierarchy fileHierarchy) {
  }
}