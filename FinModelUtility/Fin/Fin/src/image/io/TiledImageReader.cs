using System;

using fin.image.io.tile;

using schema.binary;

using SixLabors.ImageSharp.PixelFormats;

namespace fin.image.io;

public static class TiledImageReader {
  public static TiledImageReader<TPixel> New<TPixel>(
      int width,
      int height,
      int tileWidth,
      int tileHeight,
      IPixelReader<TPixel> pixelReader)
      where TPixel : unmanaged, IPixel<TPixel>
    => New(width,
           height,
           tileWidth,
           tileHeight,
           new BasicPixelIndexer(tileWidth),
           pixelReader);

  public static TiledImageReader<TPixel> New<TPixel>(
      int width,
      int height,
      int tileWidth,
      int tileHeight,
      IPixelIndexer pixelIndexer,
      IPixelReader<TPixel> pixelReader)
      where TPixel : unmanaged, IPixel<TPixel>
    => New(width,
           height,
           new BasicTileReader<TPixel>(
               tileWidth,
               tileHeight,
               pixelIndexer,
               pixelReader));

  public static TiledImageReader<TPixel> New<TPixel>(
      int width,
      int height,
      ITileReader<TPixel> tileReader)
      where TPixel : unmanaged, IPixel<TPixel>
    => new(width,
           height,
           tileReader);
}

public class TiledImageReader<TPixel>(
    int width,
    int height,
    ITileReader<TPixel> tileReader,
    Endianness endianness = Endianness.LittleEndian)
    : IImageReader<IImage<TPixel>>
    where TPixel : unmanaged, IPixel<TPixel> {
  private readonly Endianness endianness_ = endianness;

  public IImage<TPixel> ReadImage(
      byte[] srcBytes,
      Endianness endianness = Endianness.LittleEndian) {
    using var br = new SchemaBinaryReader(srcBytes, endianness);
    return this.ReadImage(br);
  }

  public IImage<TPixel> ReadImage(IBinaryReader br) {
    var image = tileReader.CreateImage(width, height);
    using var imageLock = image.Lock();
    var scan0 = imageLock.Pixels;

    var tileXCount
        = (int) Math.Ceiling(1f * width / tileReader.TileWidth);
    var tileYCount
        = (int) Math.Ceiling(1f * height / tileReader.TileHeight);

    for (var tileY = 0; tileY < tileYCount; ++tileY) {
      for (var tileX = 0; tileX < tileXCount; ++tileX) {
        tileReader.Decode(br,
                                scan0,
                                tileX,
                                tileY,
                                width,
                                height);
      }
    }

    return image;
  }
}