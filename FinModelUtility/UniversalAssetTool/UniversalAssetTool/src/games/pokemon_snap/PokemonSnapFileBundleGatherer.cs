using fin.archives;
using fin.io;
using fin.io.bundles;
using fin.util.progress;

using pokemonSnap.api;

namespace uni.games.pokemon_snap;

public sealed class PokemonSnapFileBundleGatherer : BN64FileBundleGatherer {
  public override string Name => "pokemon_snap";

  protected override void ExtractFilesFromRom(
      IReadOnlyTreeFile romFile,
      ISystemDirectory extractedDir,
      ISystemDirectory prereqsDir)
    => new PokemonSnapFileTableImporter().ExtractInto(
        new PokemonSnapRomFileBundle(romFile),
        extractedDir);

  protected override void GatherFileBundlesFromHierarchy(
      IFileBundleOrganizer organizer,
      IMutablePercentageProgress mutablePercentageProgress,
      IFileHierarchy fileHierarchy) {
    var root = fileHierarchy.Root;
  }
}