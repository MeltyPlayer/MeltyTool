namespace uni.games.beetle_adventure_racing;

public sealed class BeetleAdventureRacingMassExporter : IMassExporter {
  public void ExportAll()
    => ExporterUtil.ExportAllForCli(
        new BeetleAdventureRacingFileBundleGatherer());
}