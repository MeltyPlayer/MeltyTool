using fin.io;
using fin.io.archive;
using fin.util.strings;

using schema.binary;

using uni.platforms.threeDs.tools.gar.schema;

using LzssDecompressor = fin.compression.LzssDecompressor;

namespace uni.platforms.threeDs.tools.gar;

public class GarReader : IArchiveReader<SubArchiveContentFile> {
  public bool IsValidArchive(Stream archive) => true;

  public IArchiveStream<SubArchiveContentFile> Decompress(Stream archive) {
    if (!MagicTextUtil.Verify(archive, "LzS" + AsciiUtil.GetChar(0x1))) {
      return new SubArchiveStream(archive);
    }

    var br = new SchemaBinaryReader(archive);
    var isCompressed =
        new LzssDecompressor().TryToDecompress(br, out var decompressedGar);

    archive.Position = 0;

    return new SubArchiveStream(
        isCompressed ? new MemoryStream(decompressedGar!) : archive);
  }

  public IEnumerable<SubArchiveContentFile> GetFiles(
      IArchiveStream<SubArchiveContentFile> archiveStream) {
    var br = archiveStream.AsBinaryReader(Endianness.LittleEndian);
    var gar = new Gar(br);

    foreach (var fileType in gar.FileTypes) {
      foreach (var file in fileType.Files) {
        var fileName = file.FullPath ?? file.FileName;

        if (!fileName.EndsWith($".{fileType.TypeName}", StringComparison.OrdinalIgnoreCase)) {
          fileName = $"{fileName}.{fileType.TypeName}";
        }

        yield return new SubArchiveContentFile {
            RelativeName = fileName,
            Position = file.Position,
            Length = file.Length,
        };
      }
    }
  }
}