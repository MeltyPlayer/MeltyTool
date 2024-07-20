using grezzo.api;

namespace uni.games.majoras_mask_3d;

public class MajorasMask3dMassExporter : IMassExporter {
  public void ExportAll()
    => ExporterUtil.ExportAllForCli(new MajorasMask3dFileBundleGatherer(),
                                    new CmbModelImporter());
}