using System;
using System.Collections.Generic;

using fin.util.asserts;
using fin.util.linq;

namespace fin.io.bundles;

public interface IFileBundle : IUiFile {
  string? GameName { get; }
  IReadOnlyTreeFile? MainFile { get; }

  IEnumerable<IReadOnlyGenericFile> Files {
    get {
      if (this.MainFile != null) {
        yield return this.MainFile;
      }
    }
  }


  IReadOnlyTreeDirectory Directory => MainFile.AssertGetParent();
  string IUiFile.RawName => this.MainFile?.Name ?? "(n/a)";

  string DisplayName => this.HumanReadableName ?? this.RawName;

  string DisplayFullPath
    => this.MainFile?.DisplayFullPath ??
       this.HumanReadableName ?? this.RawName;

  string TrueFullPath => Asserts.CastNonnull(MainFile.FullPath);
}

public interface IAnnotatedFileBundleGatherer {
  IEnumerable<IAnnotatedFileBundle> GatherFileBundles();
}

public interface IAnnotatedFileBundleGatherer<out TFileBundle> : IAnnotatedFileBundleGatherer
    where TFileBundle : IFileBundle {
  new IEnumerable<IAnnotatedFileBundle<TFileBundle>> GatherFileBundles();

  IEnumerable<IAnnotatedFileBundle> IAnnotatedFileBundleGatherer.
      GatherFileBundles()
    => this.GatherFileBundles()
           .CastTo<IAnnotatedFileBundle<TFileBundle>, IAnnotatedFileBundle>();
}

public interface IAnnotatedFileBundleGathererAccumulator : IAnnotatedFileBundleGatherer {
  IAnnotatedFileBundleGathererAccumulator Add(IAnnotatedFileBundleGatherer gatherer);

  IAnnotatedFileBundleGathererAccumulator Add(
      Func<IEnumerable<IAnnotatedFileBundle>> handler);
}

public interface IAnnotatedFileBundleGathererAccumulator<TFileBundle>
    : IAnnotatedFileBundleGatherer<TFileBundle> where TFileBundle : IFileBundle {
  IAnnotatedFileBundleGathererAccumulator<TFileBundle> Add(
      IAnnotatedFileBundleGatherer<TFileBundle> gatherer);

  IAnnotatedFileBundleGathererAccumulator<TFileBundle> Add(
      Func<IEnumerable<IAnnotatedFileBundle<TFileBundle>>> handler);
}

public interface IAnnotatedFileBundleGathererAccumulatorWithInput<TFileBundle, out T>
    : IAnnotatedFileBundleGatherer<TFileBundle>
    where TFileBundle : IFileBundle {
  IAnnotatedFileBundleGathererAccumulatorWithInput<TFileBundle, T> Add(
      IAnnotatedFileBundleGatherer<TFileBundle> gatherer);

  IAnnotatedFileBundleGathererAccumulatorWithInput<TFileBundle, T> Add(
      Func<IEnumerable<IAnnotatedFileBundle<TFileBundle>>> handler);

  IAnnotatedFileBundleGathererAccumulatorWithInput<TFileBundle, T> Add(
      Func<T, IEnumerable<IAnnotatedFileBundle<TFileBundle>>> handler);
}