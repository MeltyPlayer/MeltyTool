using System;
using System.IO;

using fin.util.strings;

namespace fin.io.bundles;

public interface IGameAndLocalPath {
  string GameName { get; }
  string LocalPath { get; }
  string GameAndLocalPath => Path.Join(this.GameName, this.LocalPath);
}

public interface IAnnotatedFileBundle
    : IGameAndLocalPath, IComparable<IAnnotatedFileBundle>, IFileBundle {
  IFileBundle FileBundle { get; }

  IFileHierarchyFile File { get; }

  FileBundleType IFileBundle.Type => this.FileBundle.Type;

  int IComparable<IAnnotatedFileBundle>.CompareTo(IAnnotatedFileBundle? other) {
    var naturalSort = StringUtil.NaturalSortInstance;

    var gameNameComparison = naturalSort.Compare(this.GameName, other.GameName);
    if (gameNameComparison != 0) {
      return gameNameComparison;
    }

    var localPathComparison
        = naturalSort.Compare(this.LocalPath, other.LocalPath);
    if (localPathComparison != 0) {
      return localPathComparison;
    }

    return this.FileBundle.Type - other.FileBundle.Type;
  }
}

public interface IAnnotatedFileBundle<out TFileBundle> : IAnnotatedFileBundle
    where TFileBundle : IFileBundle {
  TFileBundle TypedFileBundle { get; }
}

public static class AnnotatedFileBundle {
  public static IAnnotatedFileBundle<TFileBundle> Annotate<TFileBundle>(
      this TFileBundle fileBundle,
      IFileHierarchyFile file) where TFileBundle : IFileBundle
    => new AnnotatedFileBundle<TFileBundle>(fileBundle, file);
}

public class AnnotatedFileBundle<TFileBundle>(
    TFileBundle fileBundle,
    IFileHierarchyFile file)
    : IAnnotatedFileBundle<TFileBundle>
    where TFileBundle : IFileBundle {
  public IFileBundle FileBundle { get; } = fileBundle;
  public TFileBundle TypedFileBundle { get; } = fileBundle;

  public IFileHierarchyFile File => file;
  public IReadOnlyTreeFile MainFile => file;

  public string GameName => file.Hierarchy.Name;
  public string LocalPath => file.LocalPath;
}