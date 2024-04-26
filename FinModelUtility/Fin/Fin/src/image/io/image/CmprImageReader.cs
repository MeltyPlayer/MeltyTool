using System;

using fin.image.io.tile;

using schema.binary;

using SixLabors.ImageSharp.PixelFormats;

namespace fin.image.io.image {
  public class CmprImageReader : IImageReader<IImage<Rgba32>> {
    private readonly CmprTileReader tileReader_ = new();
    private readonly int width_;
    private readonly int height_;

    public CmprImageReader(int width, int height) {
      this.width_ = width;
      this.height_ = height;
    }

    public IImage<Rgba32> ReadImage(IBinaryReader br) {
      Span<ushort> shortBuffer = stackalloc ushort[2];
      Span<Rgba32> paletteBuffer = stackalloc Rgba32[4];
      Span<byte> indicesBuffer = stackalloc byte[4];

      var image = this.tileReader_.CreateImage(this.width_, this.height_);
      using var imageLock = image.Lock();
      var scan0 = imageLock.Pixels;

      var tileXCount = (int) Math.Ceiling(1f * this.width_ / this.tileReader_.TileWidth);
      var tileYCount = this.height_ / this.tileReader_.TileHeight;

      for (var tileY = 0; tileY < tileYCount; ++tileY) {
        for (var tileX = 0; tileX < tileXCount; ++tileX) {
          this.tileReader_.Decode(br,
                                  scan0,
                                  tileX,
                                  tileY,
                                  this.width_,
                                  this.height_,
                                  shortBuffer,
                                  paletteBuffer,
                                  indicesBuffer);
        }
      }

      return image;
    }
  }
}