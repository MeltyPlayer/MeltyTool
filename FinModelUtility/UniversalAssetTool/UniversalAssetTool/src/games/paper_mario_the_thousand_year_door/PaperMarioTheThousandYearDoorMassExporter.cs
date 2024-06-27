using fin.io.bundles;

using ttyd.api;

using uni.platforms.gcn;

namespace uni.games.paper_mario_the_thousand_year_door {
  using IAnnotatedTtydBundle = IAnnotatedFileBundle<TtydModelFileBundle>;

  public class PaperMarioTheThousandYearDoorFileBundleGatherer
      : IAnnotatedFileBundleGatherer<TtydModelFileBundle> {
    public IEnumerable<IAnnotatedTtydBundle> GatherFileBundles() {
      if (!new GcnFileHierarchyExtractor().TryToExtractFromGame(
              "paper_mario_the_thousand_year_door",
              out var fileHierarchy)) {
        yield break;
      }

      var modelFiles
          = fileHierarchy
            .Root
            .AssertGetExistingSubdir("a")
            .GetExistingFiles()
            .Where(f => !f.Name.Contains('.') && !f.Name.EndsWith('-'));

      foreach (var modelFile in modelFiles) {
        yield return new TtydModelFileBundle { ModelFile = modelFile }
            .Annotate(modelFile);
      }
    }
  }
}