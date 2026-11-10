using fin.io;
using fin.io.bundles;
using fin.util.progress;

using pc;

namespace uni.games.pokemon_colosseum;

// https://wiki.dolphin-emu.org/index.php?title=GC6E01
[GameIDs("GC6E01", "GC6J01", "GC6P01")]
public sealed class PokemonColosseumFileBundleGatherer
    : BGameCubeFileBundleGatherer {
  public override string Name => "pokemon_colosseum";
  public override string Title => "Pokémon Colosseum";

  public override bool IsListed => false;

  protected override void GatherFileBundlesFromHierarchy(
      IFileBundleOrganizer organizer,
      IMutablePercentageProgress mutablePercentageProgress,
      IFileHierarchy fileHierarchy) {
    var didAnyUpdate = false;

    foreach (var fsysFile in fileHierarchy.Root.GetFilesWithFileType(".fsys", true)) {
      didAnyUpdate |= new FsysExtractor().TryToExtractFilesFrom(fsysFile.Impl);
    }

    if (didAnyUpdate) {
      fileHierarchy.Root.Refresh();
    }
  }
}