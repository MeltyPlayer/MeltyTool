using fin.io;
using fin.io.bundles;
using fin.util.progress;

using ttyd.api;

namespace uni.games.paper_mario_the_thousand_year_door;

public class PaperMarioTheThousandYearDoorFileBundleGatherer
    : BGameCubeFileBundleGatherer {
  public override string Name => "paper_mario_the_thousand_year_door";

  protected override void GatherFileBundlesFromHierarchy(
      IFileBundleOrganizer organizer,
      IMutablePercentageProgress mutablePercentageProgress,
      IFileHierarchy fileHierarchy) {
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