namespace fin.io;

public static partial class FileHierarchy {
  private const bool USE_DELAYED = false;

  public static IFileHierarchy From(ISystemDirectory directory)
    => USE_DELAYED
        ? new DelayedFileHierarchy(directory)
        : new UpFrontFileHierarchy(directory);

  public static IFileHierarchy From(string name, ISystemDirectory directory)
    => USE_DELAYED
        ? new DelayedFileHierarchy(name, directory)
        : new UpFrontFileHierarchy(name, directory);
}