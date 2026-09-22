namespace uni.games.super_mario_64_ds;

public sealed class SuperMario64DsMassExporter : IMassExporter {
  public void ExportAll() => ExporterUtil.ExportAllForCli(
      new SuperMario64DsFileBundleGatherer());
}