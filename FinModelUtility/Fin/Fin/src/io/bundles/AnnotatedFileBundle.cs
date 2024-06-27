namespace fin.io.bundles;

public interface IAnnotatedFileBundle {
  IFileBundle FileBundle { get; }

  IFileHierarchyFile File { get; }
  string LocalPath { get; }
  string GameAndLocalPath { get; }
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

public class AnnotatedFileBundle<TFileBundle>(TFileBundle fileBundle,
                                              IFileHierarchyFile file)
    : IAnnotatedFileBundle<TFileBundle>
    where TFileBundle : IFileBundle {
  public IFileBundle FileBundle { get; } = fileBundle;
  public TFileBundle TypedFileBundle { get; } = fileBundle;

  public IFileHierarchyFile File => file;
  public string LocalPath => file.LocalPath;

  public string GameAndLocalPath => $"{file.Hierarchy.Name}/{this.LocalPath}";
}