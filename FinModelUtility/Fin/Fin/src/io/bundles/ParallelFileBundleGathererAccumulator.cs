using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

using CommunityToolkit.HighPerformance.Helpers;

using fin.util.progress;

namespace fin.io.bundles;

public sealed class ParallelFileBundleGathererAccumulator
    : IFileBundleGathererAccumulator<
        ParallelFileBundleGathererAccumulator> {
  private readonly DelayedSplitPercentageProgress progress_ = new();
  private readonly List<IFileBundleGatherer> gatherers_ = [];

  public ParallelFileBundleGathererAccumulator Add(
      IFileBundleGatherer gatherer)
    => this.Add(gatherer, out _);

  public ParallelFileBundleGathererAccumulator Add(
      Action<IFileBundleOrganizer, IMutablePercentageProgress> handler)
    => this.Add(handler, out _);

  public ParallelFileBundleGathererAccumulator Add(
      Action<IFileBundleOrganizer> handler)
    => this.Add(handler, out _);

  public ParallelFileBundleGathererAccumulator Add(
      IFileBundleGatherer gatherer,
      out IPercentageProgress progress) {
    progress = this.progress_.Add();
    this.gatherers_.Add(gatherer);
    return this;
  }

  public ParallelFileBundleGathererAccumulator Add(
      Action<IFileBundleOrganizer, IMutablePercentageProgress> handler,
      out IPercentageProgress progress)
    => this.Add(new FileBundleHandlerGatherer(handler), out progress);

  public ParallelFileBundleGathererAccumulator Add(
      Action<IFileBundleOrganizer> handler,
      out IPercentageProgress progress)
    => this.Add(new FileBundleHandlerGathererWithoutProgress(handler),
                out progress);

  public void GatherFileBundles(
      IFileBundleOrganizer organizer,
      IMutablePercentageProgress mutablePercentageProgress) {
    this.progress_.OnProgressChanged += (_, progress)
        => mutablePercentageProgress.ReportProgress(progress);
    this.progress_.OnComplete
        += (_, _) => mutablePercentageProgress.ReportCompletion();

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
      IReadOnlyList<IFileBundleGatherer> gatherers,
      DelayedSplitPercentageProgress splitProgresses) : IAction {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Invoke(int i)
      => gatherers[i]
          .TryToGatherAndReportCompletion(organizer, splitProgresses[i]);
  }
}