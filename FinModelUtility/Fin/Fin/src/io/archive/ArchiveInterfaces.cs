using System.Collections.Generic;
using System.IO;

using schema.binary;

namespace fin.io.archive;

public interface IArchiveContentFile {
  string RelativeName { get; }
}

public interface IArchiveStream<in TArchiveContentFile>
    where TArchiveContentFile : IArchiveContentFile {
  IBinaryReader AsBinaryReader();
  IBinaryReader AsBinaryReader(Endianness endianness);

  Stream GetContentFileStream(TArchiveContentFile archiveContentFile);

  void CopyContentFileInto(TArchiveContentFile archiveContentFile,
                           Stream dstStream);
}

public interface IArchiveReader<TArchiveContentFile>
    where TArchiveContentFile : IArchiveContentFile {
  bool IsValidArchive(Stream archive);

  IArchiveStream<TArchiveContentFile> Decompress(Stream archive);

  IEnumerable<TArchiveContentFile> GetFiles(
      IArchiveStream<TArchiveContentFile> archiveStream);
}

public enum ArchiveExtractionResult {
  FAILED,
  ALREADY_EXISTS,
  NEWLY_EXTRACTED,
}

public interface IArchiveExtractor {
  delegate void ArchiveFileProcessor(string archiveFileName, ref string relativeFileName, out bool relativeToRoot);
}

public interface IArchiveExtractor<TArchiveContentFile>
    where TArchiveContentFile : IArchiveContentFile {
  ArchiveExtractionResult TryToExtractIntoNewDirectory<TArchiveReader>(
      IReadOnlyTreeFile archive)
      where TArchiveReader : IArchiveReader<TArchiveContentFile>, new();

  ArchiveExtractionResult TryToExtractIntoNewDirectory<TArchiveReader>(
      Stream archive,
      ISystemDirectory targetDirectory)
      where TArchiveReader : IArchiveReader<TArchiveContentFile>, new();

  ArchiveExtractionResult TryToExtractIntoNewDirectory<TArchiveReader>(
      IReadOnlyTreeFile archive,
      ISystemDirectory rootDirectory,
      IArchiveExtractor.ArchiveFileProcessor? archiveFileNameProcessor = null)
      where TArchiveReader : IArchiveReader<TArchiveContentFile>, new();

  ArchiveExtractionResult TryToExtractIntoNewDirectory<TArchiveReader>(
      string archiveName,
      Stream archive,
      ISystemDirectory rootDirectory,
      ISystemDirectory targetDirectory,
      IArchiveExtractor.ArchiveFileProcessor? archiveFileNameProcessor = null)
      where TArchiveReader : IArchiveReader<TArchiveContentFile>, new();
}