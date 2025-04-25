using fin.archives;
using fin.io;

using nitro.schema.narc;

using schema.binary;

namespace nitro.api;

public record NarcArchiveBundle(IReadOnlyTreeFile NarcFile)
    : IUncompressedArchiveBundle {
  public IReadOnlyTreeFile ArchiveFile => this.NarcFile;
}

public class NarcArchiveImporter
    : BUncompressedArchiveImporter<NarcArchiveBundle> {
  protected override IEnumerable<UncompressedArchiveSubFile> EnumerateSubFiles(
      IBinaryReader archiveBr,
      NarcArchiveBundle bundle)
    => archiveBr.ReadNew<Narc>().FileEntries;
}