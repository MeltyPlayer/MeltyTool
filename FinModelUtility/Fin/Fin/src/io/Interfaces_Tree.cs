using System;
using System.Collections.Generic;

using fin.util.types;

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

[UnionCandidate]
[GenerateReadOnly]
public partial interface ITreeIoObject<TIoObject, TDirectory, TFile,
                                       TFileType>
    : IEquatable<TIoObject>
    where TIoObject :
    ITreeIoObject<TIoObject, TDirectory, TFile, TFileType>
    where TDirectory :
    ITreeDirectory<TIoObject, TDirectory, TFile, TFileType>
    where TFile : ITreeFile<TIoObject, TDirectory, TFile, TFileType> {
  new string FullPath { get; }
  new ReadOnlySpan<char> Name { get; }

  [Const]
  new TDirectory AssertGetParent();

  [Const]
  new bool TryGetParent(out TDirectory parent);

  [Const]
  new IEnumerable<TDirectory> GetAncestry();
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
  new bool IsEmpty { get; }

  [Const]
  new IEnumerable<TDirectory> GetExistingSubdirs();

  [Const]
  new TDirectory AssertGetExistingSubdir(ReadOnlySpan<char> path);

  [Const]
  new bool TryToGetExistingSubdir(ReadOnlySpan<char> path,
                                  out TDirectory outDirectory);

  [Const]
  new IEnumerable<TFile> GetExistingFiles();

  [Const]
  new TFile AssertGetExistingFile(ReadOnlySpan<char> path);

  [Const]
  new bool TryToGetExistingFile(ReadOnlySpan<char> path, out TFile outFile);

  [Const]
  new bool TryToGetExistingFileWithFileType(string pathWithoutExtension,
                                            out TFile outFile,
                                            params TFileType[] fileTypes);

  [Const]
  new IEnumerable<TFile> GetFilesWithNameRecursive(string name);

  [Const]
  new IEnumerable<TFile> GetFilesWithFileType(
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
  new TFileType FileType { get; }

  new string FullNameWithoutExtension { get; }
  new ReadOnlySpan<char> NameWithoutExtension { get; }
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