using modl.api;

namespace uni.games.super_paper_mario;

public sealed class SuperPaperMarioMassExporter : IMassExporter {
  public void ExportAll()
    => ExporterUtil.ExportAllForCli(new SuperPaperMarioFileBundleGatherer());
}