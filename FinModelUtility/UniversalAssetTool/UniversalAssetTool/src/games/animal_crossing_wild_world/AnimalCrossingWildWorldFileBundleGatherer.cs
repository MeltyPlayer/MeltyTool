using fin.io;
using fin.io.bundles;
using fin.util.progress;

namespace uni.games.animal_crossing_wild_world;

public sealed class AnimalCrossingWildWorldFileBundleGatherer
    : BDsFileBundleGatherer {
  public override string Name => "animal_crossing_wild_world";

  public override bool IsListed => false;

  protected override void GatherFileBundlesFromHierarchy(
      IFileBundleOrganizer organizer,
      IMutablePercentageProgress mutablePercentageProgress,
      IFileHierarchy fileHierarchy) { }
}