using jsystem.api;

namespace uni.games.wind_waker {
  public class WindWakerMassExporter : IMassExporter {
    public void ExportAll()
      => ExporterUtil.ExportAllForCli(new WindWakerFileBundleGatherer(),
                                  new BmdModelImporter());
  }
}