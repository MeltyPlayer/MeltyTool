using System;
using System.Collections.Generic;

using fin.util.progress;

namespace fin.io.bundles;

public class AnnotatedFileBundleGathererAccumulator
    : AnnotatedFileBundleGathererAccumulator<
        AnnotatedFileBundleGathererAccumulator>;

public class AnnotatedFileBundleGathererAccumulator<TSelf>
    : IAnnotatedFileBundleGathererAccumulator<TSelf>
    where TSelf : AnnotatedFileBundleGathererAccumulator<TSelf> {
  private readonly DelayedSplitPercentageProgress progress_ = new();
  private readonly List<IAnnotatedFileBundleGatherer> gatherers_ = [];

  public TSelf Add(IAnnotatedFileBundleGatherer gatherer)
    => this.Add(gatherer, out _);

  public TSelf Add(
      Action<IFileBundleOrganizer, IMutablePercentageProgress> handler)
    => this.Add(handler, out _);

  public TSelf Add(Action<IFileBundleOrganizer> handler)
    => this.Add(handler, out _);

  public TSelf Add(IAnnotatedFileBundleGatherer gatherer,
                   out IPercentageProgress progress) {
    progress = this.progress_.Add();
    this.gatherers_.Add(gatherer);
    return (TSelf) this;
  }

  public TSelf Add(
      Action<IFileBundleOrganizer, IMutablePercentageProgress> handler,
      out IPercentageProgress progress)
    => this.Add(new AnnotatedFileBundleHandlerGatherer(handler), out progress);

  public TSelf Add(
      Action<IFileBundleOrganizer> handler,
      out IPercentageProgress progress)
    => this.Add(new AnnotatedFileBundleHandlerGathererWithoutProgress(handler),
                out progress);

  public void GatherFileBundles(
      IFileBundleOrganizer organizer,
      IMutablePercentageProgress mutablePercentageProgress) {
    for (var i = 0; i < this.gatherers_.Count; ++i) {
      this.gatherers_[i]
          .TryToGatherAndReportCompletion(organizer, this.progress_[i]);
    }
  }
}

public class AnnotatedFileBundleGathererAccumulatorWithInput<T>(T input)
    : AnnotatedFileBundleGathererAccumulator<
          AnnotatedFileBundleGathererAccumulatorWithInput<T>>,
      IAnnotatedFileBundleGathererAccumulatorWithInput<T,
          AnnotatedFileBundleGathererAccumulatorWithInput<T>> {
  public AnnotatedFileBundleGathererAccumulatorWithInput<T> Add(
      Action<IFileBundleOrganizer, IMutablePercentageProgress, T> handler)
    => this.Add(
        new AnnotatedFileBundleHandlerGathererWithInput<T>(handler, input));

  public AnnotatedFileBundleGathererAccumulatorWithInput<T> Add(
      Action<IFileBundleOrganizer, T> handler)
    => this.Add(
        new AnnotatedFileBundleHandlerGathererWithoutProgressWithInput<T>(
            handler,
            input));
}