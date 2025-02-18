using fin.common;
using fin.io.bundles;
using fin.util.progress;

using uni.platforms.ds;

namespace uni.games.animal_crossing_wild_world;

public class AnimalCrossingWildWorldFileBundleGatherer
    : IAnnotatedFileBundleGatherer {
  public void GatherFileBundles(
      IFileBundleOrganizer organizer,
      IMutablePercentageProgress mutablePercentageProgress) {
    if (!DirectoryConstants.ROMS_DIRECTORY.TryToGetExistingFile(
            "animal_crossing_wild_world.nds",
            out var animalCrossingWildWorldRom)) {
      return;
    }

    new DsFileHierarchyExtractor().ExtractFromRom(animalCrossingWildWorldRom);
  }
}