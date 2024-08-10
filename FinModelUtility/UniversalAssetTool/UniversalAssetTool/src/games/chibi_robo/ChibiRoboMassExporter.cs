using sysdolphin.api;

namespace uni.games.chibi_robo;

public class ChibiRoboMassExporter : IMassExporter {
  public void ExportAll()
    => ExporterUtil.ExportAllForCli(new ChibiRoboFileBundleGatherer(),
                                    new DatModelImporter());
}