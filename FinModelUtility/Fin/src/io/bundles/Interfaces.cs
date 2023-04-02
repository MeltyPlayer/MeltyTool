using System;
using System.Collections.Generic;
using System.IO;

using fin.util.asserts;
using fin.util.linq;


namespace fin.io.bundles {
  public interface IFileBundle : IUiFile {
    string? GameName { get; }
    IFileHierarchyFile? MainFile { get; }

    IEnumerable<IGenericFile> Files {
      get {
        if (this.MainFile != null) {
          yield return this.MainFile;
        }
      }
    }


    IFileHierarchyDirectory Directory => MainFile.Parent!;
    string IUiFile.Name => this.MainFile?.Name ?? "(n/a)";


    string DisplayFullName {
      get {
        var mainFile = this.MainFile;
        if (mainFile != null) {
          return Path.Join("//",
                           mainFile.Root.Name,
                           mainFile.Parent!.LocalPath,
                           mainFile.Name)
                     .Replace('\\', '/');
        }

        return this.BetterName ?? this.Name;
      }
    }

    string TrueFullName => Asserts.Assert(MainFile.FullName);
  }

  public interface IFileBundleGatherer {
    IEnumerable<IFileBundle> GatherFileBundles(bool assert);
  }

  public interface IFileBundleGatherer<TFileBundle> : IFileBundleGatherer
      where TFileBundle : IFileBundle {
    new IEnumerable<TFileBundle> GatherFileBundles(bool assert);

    IEnumerable<IFileBundle> IFileBundleGatherer.
        GatherFileBundles(bool assert)
      => this.GatherFileBundles(assert).CastTo<TFileBundle, IFileBundle>();
  }

  public interface IFileBundleGathererAccumulator : IFileBundleGatherer {
    IFileBundleGathererAccumulator Add(IFileBundleGatherer gatherer);
    IFileBundleGathererAccumulator Add(Func<IEnumerable<IFileBundle>> handler);
  }

  public interface IFileBundleGathererAccumulator<TFileBundle>
      : IFileBundleGatherer<TFileBundle> where TFileBundle : IFileBundle {
    IFileBundleGathererAccumulator<TFileBundle> Add(
        IFileBundleGatherer<TFileBundle> gatherer);

    IFileBundleGathererAccumulator<TFileBundle> Add(
        Func<IEnumerable<TFileBundle>> handler);
  }

  public interface IFileBundleGathererAccumulatorWithInput<TFileBundle, out T>
      : IFileBundleGatherer<TFileBundle>
      where TFileBundle : IFileBundle {
    IFileBundleGathererAccumulatorWithInput<TFileBundle, T> Add(
        IFileBundleGatherer<TFileBundle> gatherer);

    IFileBundleGathererAccumulatorWithInput<TFileBundle, T> Add(
        Func<IEnumerable<TFileBundle>> handler);

    IFileBundleGathererAccumulatorWithInput<TFileBundle, T> Add(
        Func<T, IEnumerable<TFileBundle>> handler);
  }
}