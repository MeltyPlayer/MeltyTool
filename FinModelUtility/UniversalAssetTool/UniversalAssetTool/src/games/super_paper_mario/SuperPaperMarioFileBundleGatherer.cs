using fin.io;
using fin.io.bundles;
using fin.util.progress;

using ttyd.api;

namespace uni.games.super_paper_mario;

public sealed class SuperPaperMarioFileBundleGatherer
    : BWiiFileBundleGatherer {
  public override string Name => "super_paper_mario";
  public override string Title => "Super Paper Mario";

  protected override void GatherFileBundlesFromHierarchy(
      IFileBundleOrganizer organizer,
      IMutablePercentageProgress mutablePercentageProgress,
      IFileHierarchy fileHierarchy) {
    var modelFiles
        = fileHierarchy
          .Root
          .AssertGetExistingSubdir("files/a")
          .GetExistingFiles()
          .Where(f => !f.Name.Contains('.') && !f.Name.EndsWith('-'));

    foreach (var modelFile in modelFiles) {
      organizer.Add(new TtydModelFileBundle { ModelFile = modelFile });
    }
  }
}