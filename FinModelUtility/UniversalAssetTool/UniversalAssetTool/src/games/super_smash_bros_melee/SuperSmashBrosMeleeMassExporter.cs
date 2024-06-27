using dat.api;

namespace uni.games.super_smash_bros_melee {
  public class SuperSmashBrosMeleeMassExporter : IMassExporter {
    public void ExportAll()
      => ExporterUtil.ExportAllForCli(
          new SuperSmashBrosMeleeFileBundleGatherer(),
          new DatModelImporter());
  }
}