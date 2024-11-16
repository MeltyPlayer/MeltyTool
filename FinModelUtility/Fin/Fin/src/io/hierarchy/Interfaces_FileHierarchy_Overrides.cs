namespace fin.io;

using GROTreeDir =
    IReadOnlyTreeDirectory<IReadOnlyTreeIoObject, IReadOnlyTreeDirectory,
        IReadOnlyTreeFile, string>;

using System.Collections.Generic;
using System;

public partial interface IFileHierarchyIoObject { }

public partial interface IFileHierarchyDirectory {
  // GetExistingSubdirs
  IEnumerable<IReadOnlyTreeDirectory> GROTreeDir.GetExistingSubdirs()
    => this.GetExistingSubdirs();

  new IEnumerable<IFileHierarchyDirectory> GetExistingSubdirs();

  // GetExistingFiles
  IEnumerable<IReadOnlyTreeFile> GROTreeDir.GetExistingFiles()
    => this.GetExistingFiles();

  new IEnumerable<IFileHierarchyFile> GetExistingFiles();

  // AssertGetExistingFile
  IReadOnlyTreeFile GROTreeDir.AssertGetExistingFile(
      ReadOnlySpan<char> localPath)
    => this.AssertGetExistingFile(localPath);

  new IFileHierarchyFile AssertGetExistingFile(ReadOnlySpan<char> localPath);

  // AssertGetExistingSubdir
  IReadOnlyTreeDirectory GROTreeDir.AssertGetExistingSubdir(
      ReadOnlySpan<char> localPath)
    => this.AssertGetExistingSubdir(localPath);

  new IFileHierarchyDirectory AssertGetExistingSubdir(
      ReadOnlySpan<char> localPath);


  // TryToGetExistingSubdir
  bool GROTreeDir.TryToGetExistingSubdir(
      ReadOnlySpan<char> path,
      out IReadOnlyTreeDirectory outDirectory) {
    var returnValue =
        this.TryToGetExistingSubdir(path, out var outDir);
    outDirectory = outDir;
    return returnValue;
  }

  bool TryToGetExistingSubdir(
      ReadOnlySpan<char> path,
      out IFileHierarchyDirectory outDirectory);


  // TryToGetExistingFile
  bool GROTreeDir.TryToGetExistingFile(
      ReadOnlySpan<char> path,
      out IReadOnlyTreeFile outFile) {
    var returnValue =
        this.TryToGetExistingFile(path, out var oFile);
    outFile = oFile;
    return returnValue;
  }

  bool TryToGetExistingFile(
      ReadOnlySpan<char> path,
      out IFileHierarchyFile outFile);

  // TryToGetExistingFileWithFileType
  bool GROTreeDir.TryToGetExistingFileWithFileType(
      string pathWithoutExtension,
      out IReadOnlyTreeFile outFile,
      params string[] fileTypes) {
    var returnValue =
        this.TryToGetExistingFileWithFileType(pathWithoutExtension,
                                              out var oFile,
                                              fileTypes);
    outFile = oFile;
    return returnValue;
  }

  bool TryToGetExistingFileWithFileType(string pathWithoutExtension,
                                        out IFileHierarchyFile outFile,
                                        params string[] fileTypes);

  // GetFilesWithNameRecursive
  IEnumerable<IReadOnlyTreeFile> GROTreeDir.GetFilesWithNameRecursive(
      string name)
    => this.GetFilesWithNameRecursive(name);

  new IEnumerable<IFileHierarchyFile> GetFilesWithNameRecursive(string name);

  // GetFilesWithFileType
  IEnumerable<IReadOnlyTreeFile> GROTreeDir.GetFilesWithFileType(
      string fileType,
      bool includeSubdirs = false)
    => this.GetFilesWithFileType(fileType, includeSubdirs);

  new IEnumerable<IFileHierarchyFile> GetFilesWithFileType(
      string fileType,
      bool includeSubdirs = false);
}

public partial interface IFileHierarchyFile { }