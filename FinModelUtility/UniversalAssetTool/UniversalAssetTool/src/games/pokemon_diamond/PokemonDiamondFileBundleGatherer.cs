using fin.common;
using fin.io.bundles;
using fin.util.progress;

using uni.platforms.ds;

namespace uni.games.pokemon_diamond;

public class PokemonDiamondFileBundleGatherer
    : IAnnotatedFileBundleGatherer {
  public void GatherFileBundles(
      IFileBundleOrganizer organizer,
      IMutablePercentageProgress mutablePercentageProgress) {
    if (!DirectoryConstants.ROMS_DIRECTORY.TryToGetExistingFile(
            "pokemon_diamond.nds",
            out var pokemonDiamondRom)) {
      return;
    }

    var fileHierarchy
        = new DsFileHierarchyExtractor().ExtractFromRom(pokemonDiamondRom);

    //NarcArchiveImporter.ImportAndExtractAll(fileHierarchy);
  }
}