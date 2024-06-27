using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

using CommunityToolkit.HighPerformance.Helpers;

namespace fin.io.bundles;

public class ParallelAnnotatedFileBundleGathererAccumulator :
    IAnnotatedFileBundleGathererAccumulator {
  private readonly List<IAnnotatedFileBundleGatherer> gatherers_ = [];

  public IAnnotatedFileBundleGathererAccumulator Add(IAnnotatedFileBundleGatherer gatherer) {
    this.gatherers_.Add(gatherer);
    return this;
  }

  public IAnnotatedFileBundleGathererAccumulator Add(
      Func<IEnumerable<IAnnotatedFileBundle>> handler)
    => this.Add(new AnnotatedFileBundleHandlerGatherer(handler));

  public IEnumerable<IAnnotatedFileBundle> GatherFileBundles() {
    var results = new IEnumerable<IAnnotatedFileBundle>[this.gatherers_.Count];
    ParallelHelper.For(0,
                       this.gatherers_.Count,
                       new GathererRunner(this.gatherers_, results));
    return results.SelectMany(result => result);
  }

  private readonly struct GathererRunner(
      IReadOnlyList<IAnnotatedFileBundleGatherer> gatherers,
      IList<IEnumerable<IAnnotatedFileBundle>> results) : IAction {

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Invoke(int i) {
      try {
        results[i] = gatherers[i].GatherFileBundles();
      } catch (Exception e) {
        results[i] = Enumerable.Empty<IAnnotatedFileBundle>();
        Console.Error.WriteLine(e.ToString());
      }
    }
  }
}