using fin.io.bundles;
using fin.util.progress;
using fin.util.types;

using uni.config;

namespace uni.games;

public class RootFileBundleGatherer {
  public IFileBundleDirectory GatherAllFiles(
      IMutablePercentageProgress mutablePercentageProgress,
      out IReadOnlyList<(INamedAnnotatedFileBundleGatherer gatherer,
          IPercentageProgress progress)> gatherersAndProgresses) {
    var gatherers
        = TypesUtil.InstantiateAllImplementationsWithDefaultConstructor<
                       INamedAnnotatedFileBundleGatherer>()
                   .OrderBy(g => g.Name)
                   .ToArray();

    var mutableGatherersAndProgresses
        = new (INamedAnnotatedFileBundleGatherer, IPercentageProgress)[gatherers
            .Length];
    gatherersAndProgresses = mutableGatherersAndProgresses;

    IAnnotatedFileBundleGatherer rootGatherer;
    if (Config.Instance.Extractor.ExtractRomsInParallel) {
      var accumulator = new ParallelAnnotatedFileBundleGathererAccumulator();
      for (var i = 0; i < gatherers.Length; i++) {
        var gatherer = gatherers[i];
        accumulator.Add(gatherer, out var progress);
        mutableGatherersAndProgresses[i] = (gatherer, progress);
      }

      rootGatherer = accumulator;
    } else {
      var accumulator = new AnnotatedFileBundleGathererAccumulator();
      for (var i = 0; i < gatherers.Length; i++) {
        var gatherer = gatherers[i];
        accumulator.Add(gatherer, out var progress);
        mutableGatherersAndProgresses[i] = (gatherer, progress);
      }

      rootGatherer = accumulator;
    }

    var organizer = new FileBundleTreeOrganizer();
    rootGatherer.GatherFileBundles(organizer, mutablePercentageProgress);
    return organizer.CleanUpAndGetRoot();
  }
}