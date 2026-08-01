using fin.io;
using fin.io.bundles;
using fin.util.progress;

namespace uni.games.pokemon_diamond;

public sealed class PokemonDiamondFileBundleGatherer
    : BDsFileBundleGatherer {
  public override string Name => "pokemon_diamond";

  public override bool IsListed => false;

  protected override void GatherFileBundlesFromHierarchy(
      IFileBundleOrganizer organizer,
      IMutablePercentageProgress mutablePercentageProgress,
      IFileHierarchy fileHierarchy) { }
}