using System;
using System.Collections.Generic;

namespace fin.io;

using GROTreeIoObj =
    IReadOnlyTreeIoObject<IReadOnlyTreeIoObject, IReadOnlyTreeDirectory,
        IReadOnlyTreeFile, string>;
using GROTreeDir =
    IReadOnlyTreeDirectory<IReadOnlyTreeIoObject, IReadOnlyTreeDirectory,
        IReadOnlyTreeFile, string>;
using GROTreeFile =
    IReadOnlyTreeFile<IReadOnlyTreeIoObject, IReadOnlyTreeDirectory,
        IReadOnlyTreeFile, string>;
using GROSysIoObj =
    IReadOnlyTreeIoObject<IReadOnlySystemIoObject, IReadOnlySystemDirectory,
        IReadOnlySystemFile, string>;
using GROSysDir =
    IReadOnlyTreeDirectory<IReadOnlySystemIoObject, IReadOnlySystemDirectory,
        IReadOnlySystemFile, string>;
using GROSysFile =
    IReadOnlyTreeFile<IReadOnlySystemIoObject, IReadOnlySystemDirectory,
        IReadOnlySystemFile, string>;
using GMSysFile =
    IReadOnlyTreeFile<ISystemIoObject, ISystemDirectory, ISystemFile, string>;

// ReadOnly
public partial interface IReadOnlySystemIoObject {
  // FullPath
  string GROSysIoObj.FullPath => this.FullPath;
  new string FullPath { get; }

  // Name
  ReadOnlySpan<char> GROSysIoObj.Name => this.Name;
  new ReadOnlySpan<char> Name { get; }

  // AssertGetParent
  IReadOnlyTreeDirectory GROTreeIoObj.AssertGetParent()
    => this.AssertGetParent();

  new IReadOnlySystemDirectory AssertGetParent();

  // GetAncestry
  IEnumerable<IReadOnlyTreeDirectory> GROTreeIoObj.GetAncestry()
    => this.GetAncestry();

  new IEnumerable<IReadOnlySystemDirectory> GetAncestry();
}

public partial interface IReadOnlySystemDirectory {
  // IsEmpty
  new bool IsEmpty { get; }

  // GetExistingSubdirs
  IEnumerable<IReadOnlyTreeDirectory> GROTreeDir.GetExistingSubdirs()
    => this.GetExistingSubdirs();

  IEnumerable<IReadOnlySystemDirectory> GROSysDir.GetExistingSubdirs()
    => this.GetExistingSubdirs();

  new IEnumerable<IReadOnlySystemDirectory> GetExistingSubdirs();

  // AssertGetExistingSubdir
  IReadOnlyTreeDirectory GROTreeDir.AssertGetExistingSubdir(
      ReadOnlySpan<char> path)
    => this.AssertGetExistingSubdir(path);

  IReadOnlySystemDirectory GROSysDir.AssertGetExistingSubdir(
      ReadOnlySpan<char> path)
    => this.AssertGetExistingSubdir(path);

  new IReadOnlySystemDirectory AssertGetExistingSubdir(ReadOnlySpan<char> path);

  // TryToGetExistingSubdir
  bool GROTreeDir.TryToGetExistingSubdir(
      ReadOnlySpan<char> path,
      out IReadOnlyTreeDirectory outDirectory) {
    var returnValue = this.TryToGetExistingSubdir(path, out var outDir);
    outDirectory = outDir;
    return returnValue;
  }

  new bool TryToGetExistingSubdir(ReadOnlySpan<char> path,
                                  out IReadOnlySystemDirectory outDirectory);

  // GetExistingFiles
  IEnumerable<IReadOnlyTreeFile> GROTreeDir.GetExistingFiles()
    => this.GetExistingFiles();

  IEnumerable<IReadOnlySystemFile> GROSysDir.GetExistingFiles()
    => this.GetExistingFiles();

  new IEnumerable<IReadOnlySystemFile> GetExistingFiles();

  // AssertGetExistingFile
  IReadOnlyTreeFile GROTreeDir.AssertGetExistingFile(ReadOnlySpan<char> path)
    => this.AssertGetExistingFile(path);

  new IReadOnlySystemFile AssertGetExistingFile(ReadOnlySpan<char> path);

  // TryToGetExistingFile
  bool GROTreeDir.TryToGetExistingFile(ReadOnlySpan<char> path,
                                       out IReadOnlyTreeFile outFile) {
    var returnValue = this.TryToGetExistingFile(path, out var oFile);
    outFile = oFile;
    return returnValue;
  }

  bool GROSysDir.TryToGetExistingFile(ReadOnlySpan<char> path,
                                      out IReadOnlySystemFile outFile) {
    var returnValue = this.TryToGetExistingFile(path, out var oFile);
    outFile = oFile;
    return returnValue;
  }

  new bool TryToGetExistingFile(ReadOnlySpan<char> path,
                                out IReadOnlySystemFile outFile);

  // GetFilesWithNameRecursive
  IEnumerable<IReadOnlyTreeFile> GROTreeDir.GetFilesWithNameRecursive(
      string name)
    => this.GetFilesWithNameRecursive(name);

  new IEnumerable<IReadOnlySystemFile> GetFilesWithNameRecursive(string name);

  // GetFilesWithFileType
  IEnumerable<IReadOnlyTreeFile> GROTreeDir.GetFilesWithFileType(
      string fileType,
      bool includeSubdirs)
    => this.GetFilesWithFileType(fileType, includeSubdirs);

  new IEnumerable<IReadOnlySystemFile> GetFilesWithFileType(
      string fileType,
      bool includeSubdirs = false);
}

public partial interface IReadOnlySystemFile {
  // FileType
  string GROTreeFile.FileType => this.FileType;
  string GROSysFile.FileType => this.FileType;
  new string FileType { get; }

  // FullNameWithoutExtension
  string GROTreeFile.FullNameWithoutExtension
    => this.FullNameWithoutExtension;

  string GROSysFile.FullNameWithoutExtension => this.FullNameWithoutExtension;
  new string FullNameWithoutExtension { get; }

  // NameWithoutExtension
  ReadOnlySpan<char> GROTreeFile.NameWithoutExtension
    => this.NameWithoutExtension;

  ReadOnlySpan<char> GROSysFile.NameWithoutExtension
    => this.NameWithoutExtension;

  new ReadOnlySpan<char> NameWithoutExtension { get; }
}

// Mutable
public partial interface ISystemIoObject {
  new string FullPath { get; }
  new ReadOnlySpan<char> Name { get; }
  new bool Exists { get; }
  new ReadOnlySpan<char> GetParentFullPath();

  // AssertGetParent
  IReadOnlySystemDirectory GROSysIoObj.AssertGetParent()
    => this.AssertGetParent();

  IReadOnlySystemDirectory IReadOnlySystemIoObject.AssertGetParent()
    => this.AssertGetParent();

  new ISystemDirectory AssertGetParent();

  // TryGetParent
  bool GROTreeIoObj.TryGetParent(out IReadOnlyTreeDirectory outParent) {
    var returnValue = this.TryGetParent(out var oParent);
    outParent = oParent;
    return returnValue;
  }

  bool GROSysIoObj.TryGetParent(out IReadOnlySystemDirectory outParent) {
    var returnValue = this.TryGetParent(out var oParent);
    outParent = oParent;
    return returnValue;
  }

  new bool TryGetParent(out ISystemDirectory parent);

  // GetAncestry
  IEnumerable<IReadOnlySystemDirectory> GROSysIoObj.GetAncestry()
    => this.GetAncestry();

  IEnumerable<IReadOnlySystemDirectory> IReadOnlySystemIoObject.GetAncestry()
    => this.GetAncestry();

  new IEnumerable<ISystemDirectory> GetAncestry();
}

public partial interface ISystemDirectory {
  // Overrides
  new bool IsEmpty { get; }

  // GetExistingSubdirs
  IEnumerable<IReadOnlySystemDirectory> IReadOnlySystemDirectory.
      GetExistingSubdirs() => this.GetExistingSubdirs();

  new IEnumerable<ISystemDirectory> GetExistingSubdirs();

  // AssertGetExistingSubdir
  IReadOnlySystemDirectory IReadOnlySystemDirectory.AssertGetExistingSubdir(
      ReadOnlySpan<char> path)
    => this.AssertGetExistingSubdir(path);

  new ISystemDirectory AssertGetExistingSubdir(ReadOnlySpan<char> path);

  new bool TryToGetExistingSubdir(ReadOnlySpan<char> path,
                                  out ISystemDirectory outDirectory);

  // GetExistingFiles
  IEnumerable<IReadOnlySystemFile>
      IReadOnlySystemDirectory.GetExistingFiles()
    => this.GetExistingFiles();

  new IEnumerable<ISystemFile> GetExistingFiles();

  // AssertGetExistingFile
  IReadOnlySystemFile GROSysDir.AssertGetExistingFile(ReadOnlySpan<char> path)
    => this.AssertGetExistingFile(path);

  IReadOnlySystemFile IReadOnlySystemDirectory.AssertGetExistingFile(
      ReadOnlySpan<char> path)
    => this.AssertGetExistingFile(path);

  new ISystemFile AssertGetExistingFile(ReadOnlySpan<char> path);

  // TryToGetExistingFile
  bool IReadOnlySystemDirectory.TryToGetExistingFile(ReadOnlySpan<char> path,
    out IReadOnlySystemFile outFile) {
    var returnValue =
        this.TryToGetExistingFile(path, out var outSystemFile);
    outFile = outSystemFile;
    return returnValue;
  }

  new bool TryToGetExistingFile(ReadOnlySpan<char> path,
                                out ISystemFile outFile);

  // TryToGetExistingFileWithFileType
  bool GROTreeDir.TryToGetExistingFileWithFileType(
      string pathWithoutExtension,
      out IReadOnlyTreeFile outFile,
      params string[] fileTypes)
    => this.TryToGetExistingFileWithFileType(
        pathWithoutExtension,
        out outFile,
        fileTypes);

  bool GROSysDir.TryToGetExistingFileWithFileType(
      string pathWithoutExtension,
      out IReadOnlySystemFile outFile,
      params string[] fileTypes) {
    var returnValue = this.TryToGetExistingFileWithFileType(
        pathWithoutExtension,
        out var outSystemFile,
        fileTypes);
    outFile = outSystemFile;
    return returnValue;
  }

  new bool TryToGetExistingFileWithFileType(string pathWithoutExtension,
                                            out ISystemFile outFile,
                                            params string[] fileTypes);

  // GetFilesWithNameRecursive
  IEnumerable<IReadOnlySystemFile> IReadOnlySystemDirectory.
      GetFilesWithNameRecursive(string name)
    => this.GetFilesWithNameRecursive(name);

  IEnumerable<IReadOnlySystemFile> GROSysDir.
      GetFilesWithNameRecursive(string name)
    => this.GetFilesWithNameRecursive(name);

  new IEnumerable<ISystemFile> GetFilesWithNameRecursive(string name);

  // GetFilesWithFileType
  IEnumerable<IReadOnlySystemFile>
      IReadOnlySystemDirectory.GetFilesWithFileType(
          string fileType,
          bool includeSubdirs)
    => this.GetFilesWithFileType(fileType, includeSubdirs);

  IEnumerable<IReadOnlySystemFile> GROSysDir.GetFilesWithFileType(
      string fileType,
      bool includeSubdirs)
    => this.GetFilesWithFileType(fileType, includeSubdirs);

  new IEnumerable<ISystemFile> GetFilesWithFileType(
      string fileType,
      bool includeSubdirs = false);
}

public partial interface ISystemFile {
  // FileType
  string GMSysFile.FileType => this.FileType;
  string IReadOnlySystemFile.FileType => this.FileType;
  new string FileType { get; }

  // FullNameWithoutExtension
  string GMSysFile.FullNameWithoutExtension => this.FullNameWithoutExtension;

  string IReadOnlySystemFile.FullNameWithoutExtension
    => this.FullNameWithoutExtension;

  new string FullNameWithoutExtension { get; }

  // NameWithoutExtension
  ReadOnlySpan<char> GMSysFile.NameWithoutExtension
    => this.NameWithoutExtension;

  ReadOnlySpan<char> IReadOnlySystemFile.NameWithoutExtension
    => this.NameWithoutExtension;

  new ReadOnlySpan<char> NameWithoutExtension { get; }
}