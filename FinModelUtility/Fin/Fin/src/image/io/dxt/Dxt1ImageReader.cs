using System;
using System.Buffers;

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

  public unsafe IImage<Rgba32> ReadImage(IBinaryReader br) {
    var image = this.tileReader_.CreateImage(width, height);
    using var imageLock = image.UnsafeLock();
    var scan0 = imageLock.pixelScan0;

    var tileXCount
        = (int) Math.Ceiling(1f * width / this.tileReader_.TileWidth);
    var tileYCount
        = (int) Math.Ceiling(1f * height / this.tileReader_.TileHeight);

    var tileSizeInAxis = subTileCountInAxis * subTileSizeInAxis;

    var subblockCount = tileXCount *
                        tileYCount *
                        subTileCountInAxis *
                        subTileCountInAxis;

    var allSubBlocksSize = subblockCount * 8;
    var allSubBlocksArray = ArrayPool<byte>.Shared.Rent(allSubBlocksSize);
    Span<byte> allSubblocks = allSubBlocksArray.AsSpan(0, allSubBlocksSize);
    br.ReadBytes(allSubblocks);

    Span<ushort> shortBuffer = stackalloc ushort[2];

    Span<Rgba32> paletteBuffer = stackalloc Rgba32[4];
    Span<byte> indicesBuffer = stackalloc byte[4];

    fixed (byte* allSubblocksPtr = &allSubblocks[0]) {
      fixed (ushort* shortBufferPtr = &shortBuffer[0]) {
        fixed (Rgba32* paletteBufferPtr = &paletteBuffer[0]) {
          fixed (byte* indicesBufferPtr = &indicesBuffer[0]) {
            var subblockIndex = 0;

            for (var tileY = 0; tileY < tileYCount; ++tileY) {
              for (var tileX = 0; tileX < tileXCount; ++tileX) {
                for (var j = 0; j < subTileCountInAxis; ++j) {
                  for (var i = 0; i < subTileCountInAxis; ++i) {
                    var subblockPtr = allSubblocksPtr + (subblockIndex++) * 8;

                    if (!br.IsOppositeEndiannessOfSystem) {
                      Buffer.MemoryCopy(subblockPtr, shortBufferPtr, 4, 4);
                    } else {
                      shortBufferPtr[0] = (ushort) ((subblockPtr[0] << 8) | subblockPtr[1]);
                      shortBufferPtr[1] = (ushort) ((subblockPtr[2] << 8) | subblockPtr[3]);
                    }
                    Buffer.MemoryCopy(subblockPtr + 4, indicesBufferPtr, 4, 4);

                    this.tileReader_.DecodeSubblock(
                        shortBufferPtr,
                        scan0,
                        paletteBufferPtr,
                        indicesBufferPtr,
                        tileX * tileSizeInAxis + i * subTileSizeInAxis,
                        tileY * tileSizeInAxis + j * subTileSizeInAxis,
                        width,
                        height);
                  }
                }
              }
            }
          }
        }
      }
    }

    ArrayPool<byte>.Shared.Return(allSubBlocksArray);

    return image;
  }
}