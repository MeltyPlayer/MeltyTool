using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

using CommunityToolkit.HighPerformance.Helpers;

using fin.util.progress;

namespace fin.io.bundles;

public class ParallelAnnotatedFileBundleGathererAccumulator
    : IAnnotatedFileBundleGathererAccumulator<
        ParallelAnnotatedFileBundleGathererAccumulator> {
  private readonly DelayedSplitPercentageProgress progress_ = new();
  private readonly List<IAnnotatedFileBundleGatherer> gatherers_ = [];

  public ParallelAnnotatedFileBundleGathererAccumulator Add(
      IAnnotatedFileBundleGatherer gatherer)
    => this.Add(gatherer, out _);

  public ParallelAnnotatedFileBundleGathererAccumulator Add(
      Action<IFileBundleOrganizer, IMutablePercentageProgress> handler)
    => this.Add(handler, out _);

  public ParallelAnnotatedFileBundleGathererAccumulator Add(
      Action<IFileBundleOrganizer> handler)
    => this.Add(handler, out _);

  public ParallelAnnotatedFileBundleGathererAccumulator Add(
      IAnnotatedFileBundleGatherer gatherer,
      out IPercentageProgress progress) {
    progress = this.progress_.Add();
    this.gatherers_.Add(gatherer);
    return this;
  }

  public ParallelAnnotatedFileBundleGathererAccumulator Add(
      Action<IFileBundleOrganizer, IMutablePercentageProgress> handler,
      out IPercentageProgress progress)
    => this.Add(new AnnotatedFileBundleHandlerGatherer(handler), out progress);

  public ParallelAnnotatedFileBundleGathererAccumulator Add(
      Action<IFileBundleOrganizer> handler,
      out IPercentageProgress progress)
    => this.Add(new AnnotatedFileBundleHandlerGathererWithoutProgress(handler),
                out progress);

  public void GatherFileBundles(
      IFileBundleOrganizer organizer,
      IMutablePercentageProgress mutablePercentageProgress) {
    ParallelHelper.For(
        0,
        this.gatherers_.Count,
        new GathererRunner(
            organizer,
            this.gatherers_,
            this.progress_));
  }

  private readonly struct GathererRunner(
      IFileBundleOrganizer organizer,
      IReadOnlyList<IAnnotatedFileBundleGatherer> gatherers,
      DelayedSplitPercentageProgress splitProgresses) : IAction {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Invoke(int i)
      => gatherers[i]
          .TryToGatherAndReportCompletion(organizer, splitProgresses[i]);
  }
}