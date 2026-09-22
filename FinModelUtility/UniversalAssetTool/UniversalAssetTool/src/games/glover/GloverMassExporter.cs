using glo.api;

namespace uni.games.glover;

public sealed class GloverMassExporter : IMassExporter {
  public void ExportAll()
    => ExporterUtil.ExportAllForCli(new GloverFileBundleGatherer());
}