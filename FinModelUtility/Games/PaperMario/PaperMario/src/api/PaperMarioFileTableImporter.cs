using fin.archives;
using fin.compression;
using fin.io;

using pm.schema.fileTable;
using pm.schema.fileTable.maps;

using schema.binary;

namespace pm.api;

public sealed record PaperMarioRomFileBundle(IReadOnlyTreeFile MainFile)
    : ISimpleArchiveFileBundle;

/// <summary>
///   Shamelessly stolen from:
///   https://github.com/magcius/noclip.website/blob/main/src/PaperMario64/tools/extractor.ts
/// </summary>
public sealed partial class PaperMarioFileTableImporter
    : BSimpleArchiveImporter<PaperMarioRomFileBundle> {
  protected override void BuildHierarchyAndGetFileStream(
      PaperMarioRomFileBundle bundle,
      ISet<IReadOnlyGenericFile> fileSet,
      ISimpleArchiveDirectory builderRoot,
      out Stream baseStream,
      out Stream readStream) {
    baseStream = readStream = bundle.MainFile.OpenRead();

    var romBr = new SchemaBinaryReader(readStream, Endianness.BigEndian);

    {
      var assetsArchiveDir = builderRoot.AddSubdir("assets");
      var assetTableOffset = romBr.Position = 0x01E40020;
      var assets = romBr.ReadNews<Asset>(1033);
      foreach (var asset in assets) {
        if (asset.CompressedSize == asset.UncompressedSize) {
          assetsArchiveDir.AddFile(
              asset.Name,
              assetTableOffset + asset.CompressedDataOffset,
              asset.CompressedSize);
        } else {
          assetsArchiveDir.AddFile(
              asset.Name,
              assetTableOffset + asset.CompressedDataOffset,
              asset.CompressedSize,
              compressedStream => {
                var uncompressedStream
                    = new MemoryStream((int) asset.UncompressedSize);
                Yay0Dec.Decompress(compressedStream, uncompressedStream);
                uncompressedStream.Position = 0;

                return uncompressedStream;
              });
        }
      }

      var areasArchiveDir = builderRoot.AddSubdir("areas");
      romBr.Position = MapTableUtil.AREA_TABLE_OFFSET;
      var areas = romBr.ReadNews<Area>(28);
      foreach (var area in areas) {
        var areaArchiveDir = areasArchiveDir.AddSubdir(area.AreaName);
        areaArchiveDir.AddJsonFile("area.json", area);

        foreach (var map in area.Maps) {
          var mapArchiveDir = areaArchiveDir.AddSubdir(map.MapName);
          mapArchiveDir.AddJsonFile("map.json", map);
          mapArchiveDir.AddFile(
              "romOverlay.bin",
              map.RomOverlayStartOffset,
              map.RomOverlayEndOffset -
              map.RomOverlayStartOffset);
        }
      }
    }
  }
}