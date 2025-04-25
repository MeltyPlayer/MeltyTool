using System.Collections.Generic;
using System.IO;
using System.Linq;

using fin.io;
using fin.io.bundles;
using fin.util.enumerables;

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
    var fs = fileBundle.ArchiveFile.OpenRead();
    var br = new SchemaBinaryReader(fs);

    var fileEntries = new List<UncompressedArchiveSubFileImpl>();
    var archive = new UncompressedArchive {
        Stream = fs,
        FileBundle = fileBundle,
        Files = fileBundle.Files.ToHashSet(),
        FileEntries = fileEntries,
    };

    this.EnumerateSubFiles(br, fileBundle)
        .Select(metadata
                    => new UncompressedArchiveSubFileImpl(archive, metadata))
        .AddTo(fileEntries);

    return archive;
  }

  private class UncompressedArchive : IArchive {
    public void Dispose() => this.Stream.Dispose();

    public required Stream Stream { get; init; }
    public required IFileBundle FileBundle { get; init; }
    public required IReadOnlySet<IReadOnlyGenericFile> Files { get; init; }
    public required IReadOnlyList<IArchiveSubFile> FileEntries { get; init; }
  }

  private class UncompressedArchiveSubFileImpl(
      UncompressedArchive archive,
      UncompressedArchiveSubFile metadata) : IArchiveSubFile {
    public string FullPath => metadata.FullPath;

    public Stream OpenRead() => new Substream(archive.Stream,
                                              metadata.Position,
                                              metadata.Length);
  }
}