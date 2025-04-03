using fin.io.bundles;
using fin.util.progress;

using gm.api;

using uni.platforms.desktop;

using vhr.api;


namespace uni.games.victory_heat_rally;

public class VictoryHeatRallyBundleGatherer : IAnnotatedFileBundleGatherer {
  public void GatherFileBundles(
      IFileBundleOrganizer organizer,
      IMutablePercentageProgress mutablePercentageProgress) {
    if (!SteamUtils.TryGetGameDirectory("Victory Heat Rally",
                                        out var vhrSteamDirectory)) {
      return;
    }

    var fileHierarchy =
        ExtractorUtil.GetFileHierarchy("victory_heat_rally", vhrSteamDirectory);
    var dataWin = fileHierarchy.Root.AssertGetExistingFile("data.win");

    var scratchDirectory =
        ExtractorUtil.GetOrCreateExtractedDirectory("victory_heat_rally");
    new DataWinExtractor().Extract(
        dataWin,
        scratchDirectory.GetOrCreateSubdir("dataWin"));

    var dataDirectory = fileHierarchy.Root.AssertGetExistingSubdir("data");
    foreach (var vbuffFile in dataDirectory
                              .AssertGetExistingSubdir("TRK\\MODEL")
                              .GetExistingFiles()) {
      organizer.Add(new VbModelFileBundle(vbuffFile).Annotate(vbuffFile));
    }

    foreach (var jsonFile in dataDirectory.AssertGetExistingSubdir("TRK")
                                          .GetExistingFiles()) {
      organizer.Add(new VictoryHeatRallyTrackSceneFileBundle {
          TrackJsonFile = jsonFile,
          ExtractedDirectory = scratchDirectory,
          DataDirectory = dataDirectory,
      }.Annotate(jsonFile));
    }
  }
}