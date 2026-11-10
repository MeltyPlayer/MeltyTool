using fin.io;
using fin.io.bundles;
using fin.util.progress;

using ttyd.api;

namespace uni.games.paper_mario_the_thousand_year_door;

// https://wiki.dolphin-emu.org/index.php?title=G8ME01
[GameIDs("G8ME01", "G8MJ01", "G8MK01", "G8MP01")]
public sealed class PaperMarioTheThousandYearDoorFileBundleGatherer
    : BGameCubeFileBundleGatherer {
  public override string Name => "paper_mario_the_thousand_year_door";
  public override string Title => "Paper Mario: The Thousand-Year Door";

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
      organizer.Add(new TtydModelFileBundle { ModelFile = modelFile.Impl });
    }
  }
}