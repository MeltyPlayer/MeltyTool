using fin.image;

using schema.binary;

using SixLabors.ImageSharp.PixelFormats;

namespace gx.image {
  public class CmprImageReader(
      int width,
      int height,
      int subTileCountInAxis = 2,
      int subTileSizeInAxis = 4,
      bool flipBlocksHorizontally = true)
      : fin.image.io.IImageReader<IImage<Rgba32>> {
    private readonly CmprTileReader tileReader_
        = new(subTileCountInAxis, subTileSizeInAxis, flipBlocksHorizontally);

    public IImage<Rgba32> ReadImage(IBinaryReader br) {
      Span<ushort> shortBuffer = stackalloc ushort[2];
      Span<Rgba32> paletteBuffer = stackalloc Rgba32[4];
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