using ttyd.api;


namespace uni.games.paper_mario_the_thousand_year_door;

public sealed class PaperMarioTheThousandYearDoorMassExporter : IMassExporter {
  public void ExportAll()
    => ExporterUtil.ExportAllForCli(
        new PaperMarioTheThousandYearDoorFileBundleGatherer(),
        new TtydModelImporter());
}