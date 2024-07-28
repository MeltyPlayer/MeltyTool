using System;
using System.Collections.Generic;

using fin.util.asserts;
using fin.util.progress;

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


  IReadOnlyTreeDirectory Directory => this.MainFile.AssertGetParent();
  string IUiFile.RawName => this.MainFile?.Name ?? "(n/a)";

  string DisplayName => this.HumanReadableName ?? this.RawName;

  string DisplayFullPath
    => this.MainFile?.DisplayFullPath ??
       this.HumanReadableName ?? this.RawName;

  string TrueFullPath => Asserts.CastNonnull(this.MainFile.FullPath);
}

public interface IAnnotatedFileBundleGatherer {
  void GatherFileBundles(IFileBundleOrganizer organizer,
                         IMutablePercentageProgress mutablePercentageProgress);
}

public interface IAnnotatedFileBundleGathererAccumulator<out TSelf>
    : IAnnotatedFileBundleGatherer
    where TSelf : IAnnotatedFileBundleGathererAccumulator<TSelf> {
  TSelf Add(IAnnotatedFileBundleGatherer gatherer);
  TSelf Add(Action<IFileBundleOrganizer, IMutablePercentageProgress> handler);
  TSelf Add(Action<IFileBundleOrganizer> handler);
}

public interface IAnnotatedFileBundleGathererAccumulatorWithInput<
    out T, out TSelf>
    : IAnnotatedFileBundleGathererAccumulator<TSelf>
    where TSelf : IAnnotatedFileBundleGathererAccumulatorWithInput<T, TSelf> {
  TSelf Add(Action<IFileBundleOrganizer, T> handler);
  TSelf Add(Action<IFileBundleOrganizer, IMutablePercentageProgress, T> handler);
}