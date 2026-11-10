using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using fin.io;
using fin.io.bundles;
using fin.util.enumerables;
using fin.util.streams;

using schema.binary;

using SubstreamSharp;

namespace fin.archives;

public record UncompressedArchiveSubFile(
    string FullPath,
    long Position,
    long Length);

public interface IUncompressedArchiveBundle : IArchiveBundle {
  IReadOnlyTreeFile ArchiveFile { get; }

  IReadOnlyTreeFile IFileBundle.MainFile => this.ArchiveFile;
}

public abstract class BUncompressedArchiveImporter<TBundle>
    : IArchiveImporter<TBundle>
    where TBundle : IUncompressedArchiveBundle {
  protected abstract IEnumerable<UncompressedArchiveSubFile> EnumerateSubFiles(
      IBinaryReader archiveBr,
      TBundle bundle);

  public IArchive Import(TBundle fileBundle) {
    var (fs, archive) = this.ImportImpl_(fileBundle);
    fs.Dispose();

    return archive;
  }

  public void ImportAndExtractRelativeTo(
      TBundle bundle,
      ISystemDirectory directory,
      bool cleanUp = false) {
    var (fs, archive) = this.ImportImpl_(bundle);

    foreach (var fileEntry in archive.TypedFileEntries) {
      var dstFile
          = new FinFile(Path.Join(directory.FullPath, fileEntry.FullPath));

      var dstDir = dstFile.AssertGetParent();
      dstDir.Create();

      using var fw = dstFile.OpenWrite();

      var metadata = fileEntry.Metadata;
      fs.CopyTo((uint) metadata.Position, (int) metadata.Length, fw);
    }

    if (cleanUp) {
      new FinFile(bundle.ArchiveFile).Delete();
    }
  }

  private (Stream, UncompressedArchive) ImportImpl_(TBundle bundle) {
    var openStream = () => bundle.ArchiveFile.OpenRead();
    var fs = openStream();

    var br = new SchemaBinaryReader(fs);

    var fileEntries = new List<UncompressedArchiveSubFileImpl>();
    var archive = new UncompressedArchive {
        FileBundle = bundle,
        Files = bundle.Files.ToHashSet(),
        TypedFileEntries = fileEntries,
        OpenStream = openStream,
    };

    this.EnumerateSubFiles(br, bundle)
        .Select(metadata
                    => new UncompressedArchiveSubFileImpl(archive, metadata))
        .AddTo(fileEntries);

    return (fs, archive);
  }

  private sealed class UncompressedArchive : IArchive {
    public void Dispose() { }

    public required IFileBundle FileBundle { get; init; }
    public required IReadOnlySet<IReadOnlyStandaloneFile> Files { get; init; }

    public IReadOnlyList<IArchiveSubFile> FileEntries => this.TypedFileEntries;

    public required IReadOnlyList<UncompressedArchiveSubFileImpl>
        TypedFileEntries { get; init; }

    public required Func<Stream> OpenStream { get; init; }
  }

  private sealed class UncompressedArchiveSubFileImpl(
      UncompressedArchive archive,
      UncompressedArchiveSubFile metadata) : IArchiveSubFile {
    public string FullPath => metadata.FullPath;
    public UncompressedArchiveSubFile Metadata => metadata;

    public Stream OpenRead() {
      var archiveStream = archive.OpenStream();
      return new Substream(archiveStream,
                           metadata.Position,
                           metadata.Length);
    }
  }
}