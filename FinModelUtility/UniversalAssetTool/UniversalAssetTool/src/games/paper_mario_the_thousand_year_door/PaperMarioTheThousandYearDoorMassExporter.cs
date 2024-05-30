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

      var modelDirectoryFiles
          = fileHierarchy
            .Root
            .AssertGetExistingSubdir("a")
            .GetExistingFiles()
            .Where(f => !f.Name.Contains('.'))
            .ToArray();

      var textureFileMap
          = modelDirectoryFiles
              .Where(f => f.Name.EndsWith('-'))
              .ToDictionary(f => f.Name[..^1]);

      foreach (var modelFile in modelDirectoryFiles.Where(
                   f => !f.Name.EndsWith('-'))) {
        textureFileMap.TryGetValue(modelFile.Name, out var textureFile);

        yield return new TtydModelFileBundle {
            ModelFile = modelFile,
            TextureFile = textureFile,
        }.Annotate(modelFile);
      }
    }
  }
}