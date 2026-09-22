using hm64.api;

namespace uni.games.harvest_moon_64;

public sealed class HarvestMoon64MassExporter : IMassExporter {
  public void ExportAll()
    => ExporterUtil.ExportAllForCli(new HarvestMoon64FileBundleGatherer());
}