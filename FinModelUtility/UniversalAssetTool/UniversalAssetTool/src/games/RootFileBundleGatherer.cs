using fin.io.bundles;
using fin.util.gc;
using fin.util.progress;

using uni.config;

namespace uni.games;

public sealed class RootFileBundleGatherer {
  public IFileBundleDirectory GatherAllFiles(
      IMutablePercentageProgress mutablePercentageProgress,
      out IReadOnlyList<(INamedFileBundleGatherer gatherer,
          IPercentageProgress progress)> gatherersAndProgresses) {
    var configuredExtractors
        = Config.Instance.Extractor.GamesToExtract?.Where(p => p.Value)
                .Select(p => p.Key)
                .ToHashSet() ?? [];

    var enabledGatherers = new List<INamedFileBundleGatherer>();
    foreach (var gatherer in ExtractorUtil.GetAllExtractors()) {
      if (!gatherer.IsListed || configuredExtractors.Contains(gatherer.Name)) {
        enabledGatherers.Add(gatherer);
      }
    }

    var mutableGatherersAndProgresses
        = new (INamedFileBundleGatherer, IPercentageProgress)[enabledGatherers
            .Count];
    gatherersAndProgresses = mutableGatherersAndProgresses;

    IFileBundleGatherer rootGatherer;
    if (Config.Instance.Extractor.ExtractRomsInParallel) {
      var accumulator = new ParallelFileBundleGathererAccumulator();
      for (var i = 0; i < enabledGatherers.Count; i++) {
        var gatherer = enabledGatherers[i];
        accumulator.Add(gatherer, out var progress);
        mutableGatherersAndProgresses[i] = (gatherer, progress);
      }

      rootGatherer = accumulator;
    } else {
      var accumulator = new FileBundleGathererAccumulator();
      for (var i = 0; i < enabledGatherers.Count; i++) {
        var gatherer = enabledGatherers[i];
        accumulator.Add(gatherer, out var progress);
        mutableGatherersAndProgresses[i] = (gatherer, progress);
      }

      rootGatherer = accumulator;
    }

    var organizer = new FileBundleTreeOrganizer();
    rootGatherer.GatherFileBundles(organizer, mutablePercentageProgress);
    var root = organizer.CleanUpAndGetRoot();
    GcUtil.ForceCollectEverything(true);
    return root;
  }
}