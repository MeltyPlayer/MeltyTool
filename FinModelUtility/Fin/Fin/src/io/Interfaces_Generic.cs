using System.IO.Abstractions;

using schema.readOnly;

namespace fin.io;

[GenerateReadOnly]
public partial interface IGenericFile {
  new string DisplayFullPath { get; }

  [Const]
  new FileSystemStream OpenRead();

  FileSystemStream OpenWrite();
}