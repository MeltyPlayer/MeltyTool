using System.IO.Compression;

using fin.archives;
using fin.io;

using schema.binary;

namespace dk64.api;

public sealed record Dk64RomFileBundle(IReadOnlyTreeFile MainFile)
    : ISimpleArchiveFileBundle;

/// <summary>
///   Shamelessly stolen from:
///   https://github.com/magcius/noclip.website/blob/main/src/DonkeyKong64/tools/extractor.ts
/// </summary>
public sealed class Dk64FileTableImporter
    : BSimpleArchiveImporter<Dk64RomFileBundle> {
  private const uint POINTER_TABLE_OFFSET_ = 0x101C50;
  private const uint MAP_TABLE_OFFSET_ = 0x15232C;
  private const uint TEXTURE_TABLE_OFFSET_ = 0x118B638;

  protected override void BuildHierarchyAndGetFileStream(
      Dk64RomFileBundle bundle,
      ISet<IReadOnlyGenericFile> fileSet,
      ISimpleArchiveDirectory builderRoot,
      out Stream baseStream,
      out Stream readStream) {
    baseStream = readStream = bundle.MainFile.OpenRead();

    var romBr = new SchemaBinaryReader(readStream, Endianness.BigEndian);

    // Map data table.
    var mapsArchiveDir = builderRoot.AddSubdir("maps");
    romBr.Position = MAP_TABLE_OFFSET_;
    var mapDataPointers = romBr.ReadUInt32s(0xd8);
    for (var i = 0; i < mapDataPointers.Length; ++i) {
      var mapDataPointer = mapDataPointers[i];

      var offset = POINTER_TABLE_OFFSET_ + (mapDataPointer & 0x7FFFFFFF);
      if ((mapDataPointer & 0x80000000) != 0) {
        // Indirect reference to another map.
        var otherMap = romBr.SubreadAt(offset, () => romBr.ReadUInt16());
        // TODO: What to do with this?
      } else {
        romBr.Position = offset;
        romBr.AssertUInt32(0x1F8B0800);

        mapsArchiveDir.AddFile(
            $"map{i}.bin",
            offset + 0xa,
            DecompressDataAtCurrentOffset_);
      }
    }

    // Texture data table.
    var texturesArchiveDir = builderRoot.AddSubdir("textures");
    romBr.Position = TEXTURE_TABLE_OFFSET_;
    var texDataPointers = romBr.ReadUInt32s(0xc00);
    for (var i = 0; i < texDataPointers.Length; ++i) {
      var texDataPointer = texDataPointers[i];

      var offset = POINTER_TABLE_OFFSET_ + (texDataPointer & 0x7FFFFFFF);

      romBr.Position = offset;
      romBr.AssertUInt32(0x1F8B0800);

      texturesArchiveDir.AddFile(
          $"texData{i}.bin",
          offset + 0xa,
          DecompressDataAtCurrentOffset_);
    }
  }

  private static Stream DecompressDataAtCurrentOffset_(Stream src) {
    var ms = new MemoryStream();

    var zlibStream = new DeflateStream(src, CompressionMode.Decompress, true);
    Span<byte> buffer = stackalloc byte[2048];
    while (true) {
      int size = zlibStream.Read(buffer);
      if (size > 0) {
        ms.Write(buffer[..size]);
      } else {
        break;
      }
    }

    ms.Position = 0;
    return ms;
  }
}