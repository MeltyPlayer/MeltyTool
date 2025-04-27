using fin.archives;
using fin.common;
using fin.config;
using fin.io.bundles;
using fin.util.progress;

using nitro.api;

using uni.platforms.ds;

namespace uni.games.pokemon_heartgold;

public class PokemonHeartgoldFileBundleGatherer
    : IAnnotatedFileBundleGatherer {
  public void GatherFileBundles(
      IFileBundleOrganizer organizer,
      IMutablePercentageProgress mutablePercentageProgress) {
    if (!DirectoryConstants.ROMS_DIRECTORY.TryToGetExistingFile(
            "pokemon_heartgold.nds",
            out var pokemonHeartgoldRom)) {
      return;
    }

    var fileHierarchy
        = new DsFileHierarchyExtractor().ExtractFromRom(pokemonHeartgoldRom);

    //NarcArchiveImporter.ImportAndExtractAll(fileHierarchy);
  }
}