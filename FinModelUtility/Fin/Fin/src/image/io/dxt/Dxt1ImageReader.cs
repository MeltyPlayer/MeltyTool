using System;
using System.Buffers;

using CommunityToolkit.HighPerformance;

using schema.binary;

using SixLabors.ImageSharp.PixelFormats;

namespace fin.image.io.dxt;

public sealed class Dxt1ImageReader(
    int width,
    int height,
    int subTileCountInAxis = 2,
    int subTileSizeInAxis = 4,
    bool flipBlocksHorizontally = true)
    : IImageReader<IImage<Rgba32>> {
  private readonly Dxt1TileReader tileReader_
      = new(subTileCountInAxis, subTileSizeInAxis, flipBlocksHorizontally);

  public IImage<Rgba32> ReadImage(IBinaryReader br) {
    var image = this.tileReader_.CreateImage(width, height);
    using var imageLock = image.Lock();
    var scan0 = imageLock.Pixels;

    var tileXCount
        = (int) Math.Ceiling(1f * width / this.tileReader_.TileWidth);
    var tileYCount
        = (int) Math.Ceiling(1f * height / this.tileReader_.TileHeight);

    var tileSizeInAxis = subTileCountInAxis * subTileSizeInAxis;

    Span<ushort> shortBuffer = stackalloc ushort[2];
    var shortBufferBytes = shortBuffer.AsBytes();

    Span<Rgba32> paletteBuffer = stackalloc Rgba32[4];
    Span<byte> indicesBuffer = stackalloc byte[4];

    var subblockCount = tileXCount *
                        tileYCount *
                        subTileCountInAxis *
                        subTileCountInAxis;

    var allSubBlocksSize = subblockCount * 8;
    var allSubBlocksArray = ArrayPool<byte>.Shared.Rent(allSubBlocksSize);
    Span<byte> allSubblocks = allSubBlocksArray.AsSpan(0, allSubBlocksSize);
    br.ReadBytes(allSubblocks);

    var subblockIndex = 0;

    for (var tileY = 0; tileY < tileYCount; ++tileY) {
      for (var tileX = 0; tileX < tileXCount; ++tileX) {
        for (var j = 0; j < subTileCountInAxis; ++j) {
          for (var i = 0; i < subTileCountInAxis; ++i) {
            var subblock = allSubblocks.Slice((subblockIndex++) * 8, 8);

            if (!br.IsOppositeEndiannessOfSystem) {
              subblock.Slice(0, 4).CopyTo(shortBufferBytes);
            } else {
              shortBuffer[0] = (ushort) ((subblock[0] << 8) | subblock[1]);
              shortBuffer[1] = (ushort) ((subblock[2] << 8) | subblock[3]);
            }
            subblock.Slice(4).CopyTo(indicesBuffer);

            this.tileReader_.DecodeSubblock(
                shortBuffer,
                scan0,
                paletteBuffer,
                indicesBuffer,
                tileX * tileSizeInAxis + i * subTileSizeInAxis,
                tileY * tileSizeInAxis + j * subTileSizeInAxis,
                width,
                height);
          }
        }
      }
    }

    ArrayPool<byte>.Shared.Return(allSubBlocksArray);

    return image;
  }
}