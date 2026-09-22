using grezzo.api;

namespace uni.games.ocarina_of_time_3d;

public sealed class OcarinaOfTime3dMassExporter : IMassExporter {
  public void ExportAll()
    => ExporterUtil.ExportAllForCli(new OcarinaOfTime3dFileBundleGatherer());
}