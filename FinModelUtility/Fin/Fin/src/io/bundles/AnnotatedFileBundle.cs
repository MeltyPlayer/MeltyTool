using System;
using System.IO;

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

  string IGameAndLocalPath.GameAndLocalPath => this.GameAndLocalPath;

  int IComparable<IAnnotatedFileBundle>.CompareTo(IAnnotatedFileBundle? other) {
    var gameNameComparison = this.GameName.CompareTo(other.GameName);
    if (gameNameComparison != 0) {
      return gameNameComparison;
    }

    return this.LocalPath.CompareTo(other.LocalPath);
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