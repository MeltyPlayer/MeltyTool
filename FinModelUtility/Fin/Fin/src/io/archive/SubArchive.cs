using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using schema.binary;

using SubstreamSharp;

namespace fin.io.archive;

public readonly struct SubArchiveContentFile : IArchiveContentFile {
  public required string RelativeName { get; init; }
  public required int Position { get; init; }
  public required int Length { get; init; }
}

public class SubArchiveStream : IArchiveStream<SubArchiveContentFile> {
  private readonly Stream impl_;

  public SubArchiveStream(Stream impl) {
    this.impl_ = impl;
  }

  public IBinaryReader AsBinaryReader()
    => new SchemaBinaryReader(this.impl_);

  public IBinaryReader AsBinaryReader(Endianness endianness)
    => new SchemaBinaryReader(this.impl_, endianness);

  public Stream GetContentFileStream(
      SubArchiveContentFile archiveContentFile)
    => this.impl_.Substream(archiveContentFile.Position,
                            archiveContentFile.Length);

  public void CopyContentFileInto(SubArchiveContentFile archiveContentFile,
                                  Stream dstStream) {
    this.impl_.Position = archiveContentFile.Position;

    Span<byte> buffer = stackalloc byte[81920];
    for (var i = 0; i < archiveContentFile.Length; i += buffer.Length) {
      var remaining = archiveContentFile.Length - i;
      var target = remaining > buffer.Length
          ? buffer
          : buffer.Slice(0, remaining);

      this.impl_.Read(target);
      dstStream.Write(target);
    }
  }
}

public class SubArchiveExtractor : IArchiveExtractor<SubArchiveContentFile> {
  public ArchiveExtractionResult TryToExtractIntoNewDirectory<TArchiveReader>(IReadOnlyTreeFile archive,
    ISystemDirectory targetDirectory) where TArchiveReader : IArchiveReader<SubArchiveContentFile>, new()
    => this.TryToExtractIntoNewDirectory<TArchiveReader>(archive, null, targetDirectory);

  public ArchiveExtractionResult TryToExtractIntoNewDirectory<TArchiveReader>(Stream archive, ISystemDirectory targetDirectory) where TArchiveReader : IArchiveReader<SubArchiveContentFile>, new()
    => this.TryToExtractIntoNewDirectory<TArchiveReader>(null, archive, null, targetDirectory);

  public ArchiveExtractionResult TryToExtractIntoNewDirectory<TArchiveReader>(
      IReadOnlyTreeFile archive,
      ISystemDirectory rootDirectory,
      ISystemDirectory targetDirectory,
      IArchiveExtractor.ArchiveFileProcessor? archiveFileNameProcessor = null)
      where TArchiveReader : IArchiveReader<SubArchiveContentFile>, new() {
    if (targetDirectory is { Exists: true, IsEmpty: false }) {
      return ArchiveExtractionResult.ALREADY_EXISTS;
    }

    using var fs = archive.OpenRead();
    return this.TryToExtractIntoNewDirectory<TArchiveReader>(
        archive.NameWithoutExtension,
        fs,
        rootDirectory,
        targetDirectory,
        archiveFileNameProcessor);
  }

  public ArchiveExtractionResult TryToExtractIntoNewDirectory<TArchiveReader>(
      string archiveName,
      Stream archive,
      ISystemDirectory rootDirectory,
      ISystemDirectory targetDirectory,
      IArchiveExtractor.ArchiveFileProcessor? archiveFileNameProcessor = null)
      where TArchiveReader : IArchiveReader<SubArchiveContentFile>, new() {
    if (targetDirectory is { Exists: true, IsEmpty: false }) {
      return ArchiveExtractionResult.ALREADY_EXISTS;
    }

    return this.TryToExtractIntoExistingDirectory_<TArchiveReader>(
        archiveName,
        archive,
        rootDirectory,
        targetDirectory,
        archiveFileNameProcessor);
  }

  private ArchiveExtractionResult TryToExtractIntoExistingDirectory_<
      TArchiveReader>(
      string archiveName,
      Stream archive,
      ISystemDirectory rootDirectory,
      ISystemDirectory targetDirectory,
      IArchiveExtractor.ArchiveFileProcessor? archiveFileNameProcessor = null)
      where TArchiveReader : IArchiveReader<SubArchiveContentFile>, new() {
    var archiveReader = new TArchiveReader();
    if (!archiveReader.IsValidArchive(archive)) {
      return ArchiveExtractionResult.FAILED;
    }

    var archiveStream = archiveReader.Decompress(archive);

    var archiveContentFiles = archiveReader.GetFiles(archiveStream);

    var createdDirectories = new HashSet<string>();
    foreach (var archiveContentFile in archiveContentFiles) {
      var relativeToRoot = false;

      var relativeName = archiveContentFile.RelativeName;
      if (archiveFileNameProcessor != null) {
        archiveFileNameProcessor(archiveName, ref relativeName, out relativeToRoot);
      }

      var dstDir = relativeToRoot ? rootDirectory : targetDirectory;
      var dstFile = new FinFile(Path.Join(dstDir.FullPath, relativeName));

      var dstDirectory = dstFile.GetParentFullPath()!;
      if (createdDirectories.Add(dstDirectory)) {
        FinFileSystem.Directory.CreateDirectory(dstDirectory);
      }

      using var dstFileStream = dstFile.OpenWrite();
      archiveStream.CopyContentFileInto(archiveContentFile, dstFileStream);
    }

    return ArchiveExtractionResult.NEWLY_EXTRACTED;
  }


  public ArchiveExtractionResult ExtractRelativeToRoot<TArchiveReader>(
      IReadOnlyGenericFile archiveFile,
      IReadOnlyTreeDirectory rootDirectory)
      where TArchiveReader : IArchiveReader<SubArchiveContentFile>, new() {
    using var archive = archiveFile.OpenRead();

    var archiveReader = new TArchiveReader();
    if (!archiveReader.IsValidArchive(archive)) {
      return ArchiveExtractionResult.FAILED;
    }

    var archiveStream = archiveReader.Decompress(archive);

    var archiveContentFiles = archiveReader.GetFiles(archiveStream).ToArray();

    var createdDirectories = new HashSet<string>();
    foreach (var archiveContentFile in archiveContentFiles) {
      var relativeName = archiveContentFile.RelativeName;

      var dstDir = rootDirectory;
      var dstFile = new FinFile(Path.Join(dstDir.FullPath, relativeName));

      var dstDirectory = dstFile.GetParentFullPath()!;
      if (createdDirectories.Add(dstDirectory)) {
        FinFileSystem.Directory.CreateDirectory(dstDirectory);
      }

      using var dstFileStream = dstFile.OpenWrite();
      archiveStream.CopyContentFileInto(archiveContentFile, dstFileStream);
    }

    return ArchiveExtractionResult.NEWLY_EXTRACTED;
  }
}