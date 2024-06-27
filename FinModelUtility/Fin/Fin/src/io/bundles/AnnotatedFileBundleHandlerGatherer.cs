using System;
using System.Collections.Generic;

namespace fin.io.bundles;

public class AnnotatedFileBundleHandlerGatherer
    : IAnnotatedFileBundleGatherer {
  private readonly Func<IEnumerable<IAnnotatedFileBundle>> impl_;

  public AnnotatedFileBundleHandlerGatherer(
      Func<IEnumerable<IAnnotatedFileBundle>> impl) {
    this.impl_ = impl;
  }

  public IEnumerable<IAnnotatedFileBundle> GatherFileBundles()
    => this.impl_();
}

public class AnnotatedFileBundleHandlerGatherer<TFileBundle>
    : IAnnotatedFileBundleGatherer<TFileBundle>
    where TFileBundle : IFileBundle {
  private readonly Func<IEnumerable<IAnnotatedFileBundle<TFileBundle>>> impl_;

  public AnnotatedFileBundleHandlerGatherer(
      Func<IEnumerable<IAnnotatedFileBundle<TFileBundle>>> impl) {
    this.impl_ = impl;
  }

  public IEnumerable<IAnnotatedFileBundle<TFileBundle>> GatherFileBundles()
    => this.impl_();
}


public class AnnotatedFileBundleHandlerGathererWithInput<TFileBundle, T>
    : IAnnotatedFileBundleGatherer<TFileBundle>
    where TFileBundle : IFileBundle {
  private readonly Func<T, IEnumerable<IAnnotatedFileBundle<TFileBundle>>>
      impl_;

  private readonly T input_;

  public AnnotatedFileBundleHandlerGathererWithInput(
      Func<T, IEnumerable<IAnnotatedFileBundle<TFileBundle>>> impl,
      T input) {
    this.impl_ = impl;
    this.input_ = input;
  }

  public IEnumerable<IAnnotatedFileBundle<TFileBundle>> GatherFileBundles()
    => this.impl_(this.input_);
}