namespace fin.io;

using GROTreeDir =
    IReadOnlyTreeDirectory<IReadOnlyTreeIoObject, IReadOnlyTreeDirectory,
        IReadOnlyTreeFile, string>;

using System.Collections.Generic;

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
  IReadOnlyTreeFile GROTreeDir.AssertGetExistingFile(string localPath)
    => this.AssertGetExistingFile(localPath);

  new IFileHierarchyFile AssertGetExistingFile(string localPath);

  // AssertGetExistingSubdir
  IReadOnlyTreeDirectory GROTreeDir.AssertGetExistingSubdir(string localPath)
    => this.AssertGetExistingSubdir(localPath);

  new IFileHierarchyDirectory AssertGetExistingSubdir(string localPath);


  // TryToGetExistingSubdir
  bool GROTreeDir.TryToGetExistingSubdir(
      string path,
      out IReadOnlyTreeDirectory outDirectory) {
    var returnValue =
        this.TryToGetExistingSubdir(path, out var outDir);
    outDirectory = outDir;
    return returnValue;
  }

  bool TryToGetExistingSubdir(
      string path,
      out IFileHierarchyDirectory outDirectory);


  // TryToGetExistingFile
  bool GROTreeDir.TryToGetExistingFile(
      string path,
      out IReadOnlyTreeFile outFile) {
    var returnValue =
        this.TryToGetExistingFile(path, out var oFile);
    outFile = oFile;
    return returnValue;
  }

  bool TryToGetExistingFile(
      string path,
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