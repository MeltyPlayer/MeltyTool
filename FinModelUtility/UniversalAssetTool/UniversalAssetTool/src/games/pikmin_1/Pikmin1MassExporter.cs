using pikmin1.api;

namespace uni.games.pikmin_1;

public class Pikmin1MassExporter : IMassExporter {
  public void ExportAll()
    => ExporterUtil.ExportAllForCli(new Pikmin1FileBundleGatherer(),
                                    new ModModelImporter());
}