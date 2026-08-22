using uni.api;

namespace uni.games.beetle_adventure_racing;

public sealed class BeetleAdventureRacingMassExporter : IMassExporter {
  public void ExportAll()
    => ExporterUtil.ExportAllOfTypeForCli(
        new BeetleAdventureRacingFileBundleGatherer(),
        new GlobalModelImporter());
}