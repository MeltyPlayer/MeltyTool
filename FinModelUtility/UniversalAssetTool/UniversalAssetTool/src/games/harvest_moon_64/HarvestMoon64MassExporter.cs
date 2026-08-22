using hm64.api;

namespace uni.games.harvest_moon_64;

public sealed class HarvestMoon64MassExporter : IMassExporter {
  public void ExportAll()
    => ExporterUtil.ExportAllOfTypeForCli(new HarvestMoon64FileBundleGatherer(),
                                          new Hm64MapModelImporter());
}