using System;
using System.Collections.Generic;

using schema.readOnly;

namespace fin.io;
// TODO: Come up with a better name for these "tree" interfaces?
// The idea is that:
// - generic files are just standalone files, don't necessarily have parents
//   - can be readonly or mutable
// - "tree" files are files that exist in a hierarchy, these may be within a file system or an archive
//   - due to the ambiguity, these are always readonly
// - system files refer to real files that exist within the file system
//   - these can be readonly or mutable

[GenerateReadOnly]
public partial interface ITreeIoObject<TIoObject, TDirectory, TFile,
                                       TFileType>
    : IEquatable<TIoObject>
    where TIoObject :
    ITreeIoObject<TIoObject, TDirectory, TFile, TFileType>
    where TDirectory :
    ITreeDirectory<TIoObject, TDirectory, TFile, TFileType>
    where TFile : ITreeFile<TIoObject, TDirectory, TFile, TFileType> {
  string FullPath { get; }
  ReadOnlySpan<char> Name { get; }

  [Const]
  TDirectory AssertGetParent();

  [Const]
  bool TryGetParent(out TDirectory parent);

  [Const]
  IEnumerable<TDirectory> GetAncestry();
}

[GenerateReadOnly]
public partial interface ITreeDirectory<TIoObject, TDirectory, TFile,
                                        TFileType>
    : ITreeIoObject<TIoObject, TDirectory, TFile, TFileType>
    where TIoObject :
    ITreeIoObject<TIoObject, TDirectory, TFile, TFileType>
    where TDirectory :
    ITreeDirectory<TIoObject, TDirectory, TFile, TFileType>
    where TFile : ITreeFile<TIoObject, TDirectory, TFile, TFileType> {
  bool IsEmpty { get; }

  [Const]
  IEnumerable<TDirectory> GetExistingSubdirs();

  [Const]
  TDirectory AssertGetExistingSubdir(ReadOnlySpan<char> path);

  [Const]
  bool TryToGetExistingSubdir(ReadOnlySpan<char> path,
                              out TDirectory outDirectory);

  [Const]
  IEnumerable<TFile> GetExistingFiles();

  [Const]
  TFile AssertGetExistingFile(ReadOnlySpan<char> path);

  [Const]
  bool TryToGetExistingFile(ReadOnlySpan<char> path, out TFile outFile);

  [Const]
  bool TryToGetExistingFileWithFileType(string pathWithoutExtension,
                                        out TFile outFile,
                                        params TFileType[] fileTypes);

  [Const]
  IEnumerable<TFile> GetFilesWithNameRecursive(string name);

  [Const]
  IEnumerable<TFile> GetFilesWithFileType(
      TFileType fileType,
      bool includeSubdirs = false);
}

[GenerateReadOnly]
public partial interface ITreeFile<TIoObject, TDirectory, TFile, TFileType>
    : ITreeIoObject<TIoObject, TDirectory, TFile, TFileType>,
      IGenericFile
    where TIoObject :
    ITreeIoObject<TIoObject, TDirectory, TFile, TFileType>
    where TDirectory :
    ITreeDirectory<TIoObject, TDirectory, TFile, TFileType>
    where TFile : ITreeFile<TIoObject, TDirectory, TFile, TFileType> {
  TFileType FileType { get; }

  string FullNameWithoutExtension { get; }
  ReadOnlySpan<char> NameWithoutExtension { get; }
}

[GenerateReadOnly]
public partial interface ITreeIoObject
    : ITreeIoObject<ITreeIoObject, ITreeDirectory, ITreeFile, string>;

[GenerateReadOnly]
public partial interface ITreeDirectory
    : ITreeIoObject,
      ITreeDirectory<ITreeIoObject, ITreeDirectory, ITreeFile, string>;

[GenerateReadOnly]
public partial interface ITreeFile
    : ITreeIoObject,
      ITreeFile<ITreeIoObject, ITreeDirectory, ITreeFile, string>;