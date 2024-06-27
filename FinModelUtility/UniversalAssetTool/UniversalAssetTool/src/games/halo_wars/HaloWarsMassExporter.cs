using hw.api;

namespace uni.games.halo_wars {
  public class HaloWarsMassExporter : IMassExporter {
    public void ExportAll()
      => ExporterUtil
          .ExportAllOfTypeForCli<IHaloWarsFileBundle, XtdModelFileBundle>(
        new HaloWarsFileBundleGatherer(),
        new XtdModelImporter());

      /*new HaloWarsTools.Program().Run(
          DirectoryConstants.ROMS_DIRECTORY.GetSubdir("halo_wars", true).FullName,
          DirectoryConstants.OUT_DIRECTORY.GetSubdir("halo_wars", true).FullName);*/
  }
}