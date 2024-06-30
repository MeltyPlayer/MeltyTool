using fin.io.bundles;
using fin.util.progress;

using ttyd.api;

using uni.platforms.gcn;

namespace uni.games.paper_mario_the_thousand_year_door;

public class PaperMarioTheThousandYearDoorFileBundleGatherer
    : IAnnotatedFileBundleGatherer {
  public void GatherFileBundles(
      IFileBundleOrganizer organizer,
      IMutablePercentageProgress mutablePercentageProgress) {
    if (!new GcnFileHierarchyExtractor().TryToExtractFromGame(
            "paper_mario_the_thousand_year_door",
            out var fileHierarchy)) {
      return;
    }

    var modelFiles
        = fileHierarchy
          .Root
          .AssertGetExistingSubdir("a")
          .GetExistingFiles()
          .Where(f => !f.Name.Contains('.') && !f.Name.EndsWith('-'));

    foreach (var modelFile in modelFiles) {
      organizer.Add(new TtydModelFileBundle { ModelFile = modelFile }
                        .Annotate(modelFile));
    }
  }
}