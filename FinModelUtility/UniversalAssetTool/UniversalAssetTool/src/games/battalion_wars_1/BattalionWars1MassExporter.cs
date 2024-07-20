using modl.api;

namespace uni.games.battalion_wars_1;

public class BattalionWars1MassExporter : IMassExporter {
  public void ExportAll()
    => ExporterUtil.ExportAllForCli(new BattalionWars1FileBundleGatherer(),
                                    new BattalionWarsModelImporter());
}