using System;
using System.Collections.Generic;

using fin.util.progress;

namespace fin.io.bundles;

public sealed class FileBundleGathererAccumulator
    : FileBundleGathererAccumulator<
        FileBundleGathererAccumulator>;

public class FileBundleGathererAccumulator<TSelf>
    : IFileBundleGathererAccumulator<TSelf>
    where TSelf : FileBundleGathererAccumulator<TSelf> {
  private readonly DelayedSplitPercentageProgress progress_ = new();
  private readonly List<IFileBundleGatherer> gatherers_ = [];

  public TSelf Add(IFileBundleGatherer gatherer)
    => this.Add(gatherer, out _);

  public TSelf Add(
      Action<IFileBundleOrganizer, IMutablePercentageProgress> handler)
    => this.Add(handler, out _);

  public TSelf Add(Action<IFileBundleOrganizer> handler)
    => this.Add(handler, out _);

  public TSelf Add(IFileBundleGatherer gatherer,
                   out IPercentageProgress progress) {
    progress = this.progress_.Add();
    this.gatherers_.Add(gatherer);
    return (TSelf) this;
  }

  public TSelf Add(
      Action<IFileBundleOrganizer, IMutablePercentageProgress> handler,
      out IPercentageProgress progress)
    => this.Add(new FileBundleHandlerGatherer(handler), out progress);

  public TSelf Add(
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

    for (var i = 0; i < this.gatherers_.Count; ++i) {
      this.gatherers_[i]
          .TryToGatherAndReportCompletion(organizer, this.progress_[i]);
    }
  }
}

public sealed class FileBundleGathererAccumulatorWithInput<T>(T input)
    : FileBundleGathererAccumulator<
          FileBundleGathererAccumulatorWithInput<T>>,
      IFileBundleGathererAccumulatorWithInput<T,
          FileBundleGathererAccumulatorWithInput<T>> {
  public FileBundleGathererAccumulatorWithInput<T> Add(
      Action<IFileBundleOrganizer, IMutablePercentageProgress, T> handler)
    => this.Add(
        new FileBundleHandlerGathererWithInput<T>(handler, input));

  public FileBundleGathererAccumulatorWithInput<T> Add(
      Action<IFileBundleOrganizer, T> handler)
    => this.Add(
        new FileBundleHandlerGathererWithoutProgressWithInput<T>(
            handler,
            input));
}