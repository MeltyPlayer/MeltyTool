using uni.api;

namespace uni.games.midnight_club_2;

public sealed class MidnightClub2MassExporter : IMassExporter {
  public void ExportAll()
    => ExporterUtil.ExportAllForCli(new MidnightClub2FileBundleGatherer());
}