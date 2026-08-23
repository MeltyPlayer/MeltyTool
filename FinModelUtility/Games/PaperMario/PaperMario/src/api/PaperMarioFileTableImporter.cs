using fin.archives;
using fin.compression;
using fin.io;

using PaperMario;

using schema.binary;

namespace pm.api;

public sealed record PaperMarioRomFileBundle(IReadOnlyTreeFile MainFile)
    : ISimpleArchiveFileBundle<PaperMarioRomFileBundle> {
  public static PaperMarioRomFileBundle FromFile(IReadOnlyTreeFile file)
    => new(file);
}

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
      var assetTableOffset = romBr.Position = 0x01E40020;
      var assetsArchiveDir = builderRoot.AddSubdir("assets");
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
    }
  }
}