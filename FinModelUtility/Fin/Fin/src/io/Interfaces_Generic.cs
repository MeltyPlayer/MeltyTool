using System.IO.Abstractions;

using schema.readOnly;

namespace fin.io;

[GenerateReadOnly]
public partial interface IGenericFile {
  string DisplayFullPath { get; }

  [Const]
  FileSystemStream OpenRead();

  FileSystemStream OpenWrite();
}