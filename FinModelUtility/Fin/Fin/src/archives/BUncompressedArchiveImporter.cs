using System;
using System.Buffers.Text;
using System.Collections.Generic;

using fin.io;

using schema.binary;

namespace fin.archives;

public record UncompressedArchiveSubFile(
    string FullPath,
    long Position,
    long Length);

public interface IUncompressedArchiveBundle : IArchiveBundle {
  IReadOnlyTreeFile ArchiveFile { get; }
}

public abstract class BUncompressedArchiveImporter<TBundle>
    : IArchiveImporter<TBundle>
    where TBundle : IUncompressedArchiveBundle {
  protected abstract IEnumerable<UncompressedArchiveSubFile> EnumerateSubFiles(
      IBinaryReader archiveBr,
      TBundle bundle);

  public IArchive Import(TBundle fileBundle) {

  }
}