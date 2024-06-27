using fin.log;

using uni.platforms.gcn;

namespace uni.games.animal_crossing;

public class AnimalCrossingMassExporter : IMassExporter {
  public void ExportAll() {
      if (!new GcnFileHierarchyExtractor().TryToExtractFromGame(
              "animal_crossing",
              out var fileHierarchy)) {
        return;
      }

      var logger = Logging.Create<AnimalCrossingMassExporter>();
      // TODO: Extract models via display lists
    }
}