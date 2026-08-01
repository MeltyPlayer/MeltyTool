using fin.io;
using fin.io.bundles;
using fin.util.progress;

using pc;

namespace uni.games.pokemon_colosseum;

public sealed class PokemonColosseumFileBundleGatherer
    : BGameCubeFileBundleGatherer {
  public override string Name => "pokemon_colosseum";

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