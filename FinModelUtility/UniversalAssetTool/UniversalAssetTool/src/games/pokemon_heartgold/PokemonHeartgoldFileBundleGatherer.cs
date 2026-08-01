using fin.io;
using fin.io.bundles;
using fin.util.progress;

namespace uni.games.pokemon_heartgold;

public sealed class PokemonHeartgoldFileBundleGatherer
    : BDsFileBundleGatherer {
  public override string Name => "pokemon_heartgold";

  public override bool IsListed => false;

  protected override void GatherFileBundlesFromHierarchy(
      IFileBundleOrganizer organizer,
      IMutablePercentageProgress mutablePercentageProgress,
      IFileHierarchy fileHierarchy) { }
}