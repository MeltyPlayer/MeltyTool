using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

using CommunityToolkit.HighPerformance.Helpers;

using fin.util.progress;

namespace fin.io.bundles;

public class ParallelAnnotatedFileBundleGathererAccumulator
    : IAnnotatedFileBundleGathererAccumulator {
  private readonly List<IAnnotatedFileBundleGatherer> gatherers_ = [];

  public IAnnotatedFileBundleGathererAccumulator Add(
      IAnnotatedFileBundleGatherer gatherer) {
    this.gatherers_.Add(gatherer);
    return this;
  }

  public IAnnotatedFileBundleGathererAccumulator Add(
      Func<IEnumerable<IAnnotatedFileBundle>> handler)
    => this.Add(new AnnotatedFileBundleHandlerGatherer(handler));

  public IEnumerable<IAnnotatedFileBundle> GatherFileBundles(
      IMutablePercentageProgress mutablePercentageProgress) {
    var splitProgresses = mutablePercentageProgress.Split(2);
    var gathererProgresses = splitProgresses[0].Split(this.gatherers_.Count);
    var loaderProgresses = splitProgresses[1].Split(this.gatherers_.Count);

    var results = new IEnumerable<IAnnotatedFileBundle>[this.gatherers_.Count];
    ParallelHelper.For(
        0,
        this.gatherers_.Count,
        new GathererRunner(
            this.gatherers_,
            gathererProgresses,
            results));

    for (var i = 0; i < this.gatherers_.Count; ++i) {
      var result = results[i];
      foreach (var value in result) {
        yield return value;
      }

      loaderProgresses[i].ReportProgressAndCompletion();
    }
  }

  private readonly struct GathererRunner(
      IReadOnlyList<IAnnotatedFileBundleGatherer> gatherers,
      SplitPercentageProgress splitProgresses,
      IList<IEnumerable<IAnnotatedFileBundle>> results) : IAction {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Invoke(int i) {
      var splitProgress = splitProgresses[i];
      try {
        results[i] = gatherers[i].GatherFileBundles(splitProgress);
      } catch (Exception e) {
        results[i] = Enumerable.Empty<IAnnotatedFileBundle>();
        Console.Error.WriteLine(e.ToString());
      } finally {
        splitProgress.ReportProgressAndCompletion();
      }
    }
  }
}