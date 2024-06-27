using System.IO.Abstractions;

using IoFile = System.IO.Abstractions.IFile;
using IoDirectory = System.IO.Abstractions.IDirectory;

namespace fin.io;

public static class FinFileSystem {
  public static IFileSystem FileSystem { get; set; } = new FileSystem();

  public static IoFile File => FileSystem.File;
  public static IoDirectory Directory => FileSystem.Directory;
}