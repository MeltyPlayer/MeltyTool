using fin.io;
using fin.io.bundles;
using fin.util.progress;

namespace uni.games.pokemon_heartgold;

public class PokemonHeartgoldFileBundleGatherer
    : BDsFileBundleGatherer {
  public override string Name => "pokemon_heartgold";

  protected override void GatherFileBundlesFromHierarchy(
      IFileBundleOrganizer organizer,
      IMutablePercentageProgress mutablePercentageProgress,
      IFileHierarchy fileHierarchy) { }
}