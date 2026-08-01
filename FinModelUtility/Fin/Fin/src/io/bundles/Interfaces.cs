using System;
using System.Collections.Generic;

using fin.util.asserts;
using fin.util.progress;
using fin.util.strings;

namespace fin.io.bundles;

public interface IFileBundle : IUiFile, IComparable<IFileBundle> {
  FileBundleType Type { get; }

  IReadOnlyTreeFile MainFile { get; }

  IEnumerable<IReadOnlyGenericFile> Files {
    get { yield return this.MainFile; }
  }

  IReadOnlyTreeDirectory Directory => this.MainFile.AssertGetParent();

  ReadOnlySpan<char> IUiFile.RawName
    => FinIoStatic.GetName(this.DisplayFullPath);

  ReadOnlySpan<char> DisplayName => this.HumanReadableName ?? this.RawName;

  ReadOnlySpan<char> DisplayFullPath => this.MainFile.DisplayFullPath;

  string TrueFullPath => Asserts.CastNonnull(this.MainFile.FullPath);

  int IComparable<IFileBundle>.CompareTo(IFileBundle? other) {
    var nameComparison = StringUtil.NaturalSortInstance.Compare(
        this.DisplayFullPath,
        other!.DisplayFullPath);
    if (nameComparison != 0) {
      return nameComparison;
    }

    return this.Type.CompareTo(other.Type);
  }
}

public interface INamedFileBundleGatherer : IFileBundleGatherer {
  string Name { get; }

  bool IsListed => true;
  bool IsAvailable { get; }
}

public interface IFileBundleGatherer {
  void GatherFileBundles(IFileBundleOrganizer organizer,
                         IMutablePercentageProgress mutablePercentageProgress);
}

public interface IFileBundleGathererAccumulator<out TSelf>
    : IFileBundleGatherer
    where TSelf : IFileBundleGathererAccumulator<TSelf> {
  TSelf Add(IFileBundleGatherer gatherer);
  TSelf Add(Action<IFileBundleOrganizer, IMutablePercentageProgress> handler);
  TSelf Add(Action<IFileBundleOrganizer> handler);

  TSelf Add(IFileBundleGatherer gatherer,
            out IPercentageProgress progress);

  TSelf Add(Action<IFileBundleOrganizer, IMutablePercentageProgress> handler,
            out IPercentageProgress progress);

  TSelf Add(Action<IFileBundleOrganizer> handler,
            out IPercentageProgress progress);
}

public interface IFileBundleGathererAccumulatorWithInput<
    out T, out TSelf>
    : IFileBundleGathererAccumulator<TSelf>
    where TSelf : IFileBundleGathererAccumulatorWithInput<T, TSelf> {
  TSelf Add(Action<IFileBundleOrganizer, T> handler);

  TSelf Add(
      Action<IFileBundleOrganizer, IMutablePercentageProgress, T> handler);
}