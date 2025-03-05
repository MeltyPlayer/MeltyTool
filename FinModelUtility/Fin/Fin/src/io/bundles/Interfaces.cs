using System;
using System.Collections.Generic;

using fin.util.asserts;
using fin.util.progress;

namespace fin.io.bundles;

public interface IFileBundle : IUiFile {
  FileBundleType Type { get; }

  IReadOnlyTreeFile? MainFile { get; }

  IEnumerable<IReadOnlyGenericFile> Files {
    get {
      if (this.MainFile != null) {
        yield return this.MainFile;
      }
    }
  }


  IReadOnlyTreeDirectory Directory => this.MainFile.AssertGetParent();

  ReadOnlySpan<char> IUiFile.RawName
    => this.MainFile != null ? this.MainFile.Name : "(n/a)";

  ReadOnlySpan<char> DisplayName => this.HumanReadableName ?? this.RawName;

  ReadOnlySpan<char> DisplayFullPath
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