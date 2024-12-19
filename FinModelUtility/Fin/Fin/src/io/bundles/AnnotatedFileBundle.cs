using System;
using System.IO;

namespace fin.io.bundles;

public interface IGameAndLocalPath {
  string GameAndLocalPath { get; }
}

public interface IAnnotatedFileBundle
    : IGameAndLocalPath, IComparable<IAnnotatedFileBundle> {
  IFileBundle FileBundle { get; }

  IFileHierarchyFile File { get; }
  string LocalPath { get; }

  string IGameAndLocalPath.GameAndLocalPath => this.GameAndLocalPath;

  int IComparable<IAnnotatedFileBundle>.CompareTo(IAnnotatedFileBundle? other) {
    var thisGameName = this.File.Hierarchy.Name;
    var otherGameName = other.File.Hierarchy.Name;
    var gameNameComparison = thisGameName.CompareTo(otherGameName);
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
  public string LocalPath => file.LocalPath;

  public string GameAndLocalPath
    => Path.Join(file.Hierarchy.Name, this.LocalPath);
}