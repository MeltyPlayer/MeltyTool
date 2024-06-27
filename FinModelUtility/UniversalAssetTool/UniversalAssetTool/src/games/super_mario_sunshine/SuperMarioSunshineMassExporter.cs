using jsystem.api;

namespace uni.games.super_mario_sunshine;

public class SuperMarioSunshineMassExporter : IMassExporter {
  public void ExportAll()
    => ExporterUtil.ExportAllForCli(new SuperMarioSunshineFileBundleGatherer(),
                                    new BmdModelImporter());
}