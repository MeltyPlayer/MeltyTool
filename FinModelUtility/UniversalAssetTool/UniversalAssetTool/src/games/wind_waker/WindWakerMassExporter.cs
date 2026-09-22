using jsystem.api;

namespace uni.games.wind_waker;

public sealed class WindWakerMassExporter : IMassExporter {
  public void ExportAll()
    => ExporterUtil.ExportAllForCli(new WindWakerFileBundleGatherer());
}