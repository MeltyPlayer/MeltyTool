using sysdolphin.api;

namespace uni.games.super_smash_bros_melee;

public sealed class SuperSmashBrosMeleeMassExporter : IMassExporter {
  public void ExportAll()
    => ExporterUtil.ExportAllForCli(
        new SuperSmashBrosMeleeFileBundleGatherer());
}