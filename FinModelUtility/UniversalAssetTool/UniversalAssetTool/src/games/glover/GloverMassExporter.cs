using glo.api;

namespace uni.games.glover;

public class GloverMassExporter : IMassExporter {
  public void ExportAll()
    => ExporterUtil.ExportAllForCli(new GloverFileBundleGatherer(),
                                    new GloModelImporter());
}