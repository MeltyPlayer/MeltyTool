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
  private readonly List<IAnnotatedFileBundleGatherer> gatherers_ = [];

  public TSelf Add(IAnnotatedFileBundleGatherer gatherer) {
    this.gatherers_.Add(gatherer);
    return (TSelf) this;
  }

  public TSelf Add(
      Action<IFileBundleOrganizer, IMutablePercentageProgress> handler)
    => this.Add(new AnnotatedFileBundleHandlerGatherer(handler));

  public TSelf Add(
      Action<IFileBundleOrganizer> handler)
    => this.Add(new AnnotatedFileBundleHandlerGathererWithoutProgress(handler));

  public void GatherFileBundles(
      IFileBundleOrganizer organizer,
      IMutablePercentageProgress mutablePercentageProgress) {
    var splitProgresses
        = mutablePercentageProgress.Split(this.gatherers_.Count);

    for (var i = 0; i < this.gatherers_.Count; ++i) {
      this.gatherers_[i]
          .TryToGatherAndReportCompletion(organizer, splitProgresses[i]);
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