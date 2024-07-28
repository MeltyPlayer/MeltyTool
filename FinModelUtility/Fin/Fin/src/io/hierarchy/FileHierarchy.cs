namespace fin.io;

public static partial class FileHierarchy {
  public static IFileHierarchy From(ISystemDirectory directory)
    => new UpFrontFileHierarchy(directory);

  public static IFileHierarchy From(string name,
                                    ISystemDirectory directory,
                                    ISystemFile cacheFile)
    => new CachedFileHierarchy(name, directory, cacheFile);
}