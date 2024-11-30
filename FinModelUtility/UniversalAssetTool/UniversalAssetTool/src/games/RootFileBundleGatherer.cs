using fin.io.bundles;
using fin.util.progress;
using fin.util.types;

using uni.config;

namespace uni.games;

public class RootFileBundleGatherer {
  public IFileBundleDirectory GatherAllFiles(
      IMutablePercentageProgress mutablePercentageProgress) {
    var gatherers
        = TypesUtil.InstantiateAllImplementationsWithDefaultConstructor<IAnnotatedFileBundleGatherer>();

    IAnnotatedFileBundleGatherer rootGatherer;
    if (Config.Instance.Extractor.ExtractRomsInParallel) {
      var accumulator = new ParallelAnnotatedFileBundleGathererAccumulator();
      foreach (var gatherer in gatherers) {
        accumulator.Add(gatherer);
      }

      rootGatherer = accumulator;
    } else {
      var accumulator = new AnnotatedFileBundleGathererAccumulator();
      foreach (var gatherer in gatherers) {
        accumulator.Add(gatherer);
      }

      rootGatherer = accumulator;
    }

    var organizer = new FileBundleTreeOrganizer();
    rootGatherer.GatherFileBundles(organizer, mutablePercentageProgress);
    return organizer.CleanUpAndGetRoot();
  }
}