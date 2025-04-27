using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

using CommunityToolkit.HighPerformance.Helpers;

using fin.util.progress;

namespace fin.io.bundles;

public class ParallelAnnotatedFileBundleGathererAccumulator
    : IAnnotatedFileBundleGathererAccumulator<
        ParallelAnnotatedFileBundleGathererAccumulator> {
  private readonly List<IAnnotatedFileBundleGatherer> gatherers_ = [];

  public ParallelAnnotatedFileBundleGathererAccumulator Add(
      IAnnotatedFileBundleGatherer gatherer) {
    this.gatherers_.Add(gatherer);
    return this;
  }

  public ParallelAnnotatedFileBundleGathererAccumulator Add(
      Action<IFileBundleOrganizer, IMutablePercentageProgress> handler)
    => this.Add(new AnnotatedFileBundleHandlerGatherer(handler));

  public ParallelAnnotatedFileBundleGathererAccumulator Add(
      Action<IFileBundleOrganizer> handler)
    => this.Add(new AnnotatedFileBundleHandlerGathererWithoutProgress(handler));

  public void GatherFileBundles(
      IFileBundleOrganizer organizer,
      IMutablePercentageProgress mutablePercentageProgress) {
    var splitProgresses = mutablePercentageProgress.Split(2);
    var gathererProgresses = splitProgresses[0].Split(this.gatherers_.Count);

    ParallelHelper.For(
        0,
        this.gatherers_.Count,
        new GathererRunner(
            organizer,
            this.gatherers_,
            gathererProgresses));
  }

  private readonly struct GathererRunner(
      IFileBundleOrganizer organizer,
      IReadOnlyList<IAnnotatedFileBundleGatherer> gatherers,
      SplitPercentageProgress splitProgresses) : IAction {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Invoke(int i)
      => gatherers[i]
          .TryToGatherAndReportCompletion(organizer, splitProgresses[i]);
  }
}