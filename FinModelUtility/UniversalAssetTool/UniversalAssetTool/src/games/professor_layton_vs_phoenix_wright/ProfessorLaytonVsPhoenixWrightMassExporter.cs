using level5.api;

namespace uni.games.professor_layton_vs_phoenix_wright;

public class ProfessorLaytonVsPhoenixWrightMassExporter : IMassExporter {
  public void ExportAll()
    => ExporterUtil.ExportAllForCli(
        new ProfessorLaytonVsPhoenixWrightFileBundleGatherer(),
        new XcModelImporter());
}