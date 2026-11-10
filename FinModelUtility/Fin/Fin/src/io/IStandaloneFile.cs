using System.IO;

using readOnly;

namespace fin.io;

[GenerateReadOnly]
public partial interface IStandaloneFile {
  new string DisplayFullPath { get; }

  [Const]
  new Stream OpenRead();

  Stream OpenWrite();
}