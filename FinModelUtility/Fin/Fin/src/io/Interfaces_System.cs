namespace fin.io;

using GROSysIoObj =
    IReadOnlyTreeIoObject<IReadOnlySystemIoObject, IReadOnlySystemDirectory,
        IReadOnlySystemFile, string>;
using GROSysDir =
    IReadOnlyTreeDirectory<IReadOnlySystemIoObject, IReadOnlySystemDirectory,
        IReadOnlySystemFile, string>;
using GROSysFile =
    IReadOnlyTreeFile<IReadOnlySystemIoObject, IReadOnlySystemDirectory,
        IReadOnlySystemFile, string>;
using GMSysIoObj =
    IReadOnlyTreeIoObject<ISystemIoObject, ISystemDirectory, ISystemFile,
        string>;
using GMSysDir =
    IReadOnlyTreeDirectory<ISystemIoObject, ISystemDirectory, ISystemFile,
        string>;
using GMSysFile =
    IReadOnlyTreeFile<ISystemIoObject, ISystemDirectory, ISystemFile, string>;
using System;

// ReadOnly
public partial interface IReadOnlySystemIoObject
    : IReadOnlyTreeIoObject, GROSysIoObj {
  bool Exists { get; }
  ReadOnlySpan<char> GetParentFullPath();
}

public partial interface IReadOnlySystemDirectory
    : IReadOnlySystemIoObject,
      IReadOnlyTreeDirectory,
      GROSysDir { }

public partial interface IReadOnlySystemFile
    : IReadOnlySystemIoObject,
      IReadOnlyTreeFile,
      GROSysFile {
  ISystemFile CloneWithFileType(string newFileType);
}


// Mutable

public partial interface ISystemIoObject
    : IReadOnlySystemIoObject, GMSysIoObj { }

public partial interface ISystemDirectory
    : ISystemIoObject, IReadOnlySystemDirectory, GMSysDir {
  bool Create();

  bool Delete(bool recursive = false);
  bool DeleteContents();

  void MoveTo(string path);

  ISystemDirectory GetOrCreateSubdir(ReadOnlySpan<char> relativePath);
}

public partial interface ISystemFile
    : ISystemIoObject, IReadOnlySystemFile, IGenericFile, GMSysFile {
  bool Delete();
}