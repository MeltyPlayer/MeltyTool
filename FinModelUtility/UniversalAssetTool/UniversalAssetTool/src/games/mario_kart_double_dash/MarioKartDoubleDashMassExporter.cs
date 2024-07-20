using jsystem.api;

namespace uni.games.mario_kart_double_dash;

public class MarioKartDoubleDashMassExporter : IMassExporter {
  public void ExportAll()
    => ExporterUtil.ExportAllForCli(new MarioKartDoubleDashFileBundleGatherer(),
                                    new BmdModelImporter());
}