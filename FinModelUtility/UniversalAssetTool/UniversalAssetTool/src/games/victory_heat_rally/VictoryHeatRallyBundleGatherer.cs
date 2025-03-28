using fin.io.bundles;
using fin.util.progress;

using gm.api;

using uni.platforms.desktop;


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

    foreach (var vbuffFile in fileHierarchy.Root.AssertGetExistingSubdir(
                                               "data\\TRK\\MODEL")
                                           .GetExistingFiles()) {
      organizer.Add(new VbModelFileBundle(vbuffFile).Annotate(vbuffFile));
    }
  }
}