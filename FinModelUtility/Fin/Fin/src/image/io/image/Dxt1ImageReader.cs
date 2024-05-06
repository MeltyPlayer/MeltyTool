using System;

using fin.image.io.tile;

using schema.binary;

using SixLabors.ImageSharp.PixelFormats;

namespace fin.image.io.image {
  public class Dxt1ImageReader(
      int width,
      int height,
      int subTileCountInAxis = 2,
      int subTileSizeInAxis = 4,
      bool flipBlocksHorizontally = true)
      : IImageReader<IImage<Rgb24>> {
    private readonly Dxt1TileReader tileReader_
        = new(subTileCountInAxis, subTileSizeInAxis, flipBlocksHorizontally);

    public IImage<Rgb24> ReadImage(IBinaryReader br) {
      Span<ushort> shortBuffer = stackalloc ushort[2];
      Span<Rgb24> paletteBuffer = stackalloc Rgb24[4];
      Span<byte> indicesBuffer = stackalloc byte[4];

      var image = this.tileReader_.CreateImage(width, height);
      using var imageLock = image.Lock();
      var scan0 = imageLock.Pixels;

      var tileXCount
          = (int) Math.Ceiling(1f * width / this.tileReader_.TileWidth);
      var tileYCount = height / this.tileReader_.TileHeight;

      for (var tileY = 0; tileY < tileYCount; ++tileY) {
        for (var tileX = 0; tileX < tileXCount; ++tileX) {
          this.tileReader_.Decode(br,
                                  scan0,
                                  tileX,
                                  tileY,
                                  width,
                                  height,
                                  shortBuffer,
                                  paletteBuffer,
                                  indicesBuffer);
        }
      }

      return image;
    }
  }
}