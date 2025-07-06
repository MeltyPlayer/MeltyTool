using fin.io;
using fin.io.bundles;
using fin.util.progress;

namespace uni.games.pokemon_diamond;

public class PokemonDiamondFileBundleGatherer
    : BDsFileBundleGatherer {
  public override string Name => "pokemon_diamond";

  protected override void GatherFileBundlesFromHierarchy(
      IFileBundleOrganizer organizer,
      IMutablePercentageProgress mutablePercentageProgress,
      IFileHierarchy fileHierarchy) { }
}