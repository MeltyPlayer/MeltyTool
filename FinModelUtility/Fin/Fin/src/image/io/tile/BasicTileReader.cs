using System;

using schema.binary;

using SixLabors.ImageSharp.PixelFormats;

namespace fin.image.io.tile;

public class BasicTileReader<TPixel>(
    int tileWidth,
    int tileHeight,
    IPixelIndexer pixelIndexer,
    IPixelReader<TPixel> pixelReader)
    : ITileReader<TPixel>
    where TPixel : unmanaged, IPixel<TPixel> {
  public IImage<TPixel> CreateImage(int width, int height)
    => pixelReader.CreateImage(width, height);

  public int TileWidth { get; } = tileWidth;
  public int TileHeight { get; } = tileHeight;

  public void Decode(IBinaryReader br,
                     Span<TPixel> scan0,
                     int tileX,
                     int tileY,
                     int imageWidth,
                     int imageHeight) {
    var xx = tileX * this.TileWidth;
    var yy = tileY * this.TileHeight;

    Span<TPixel> junk = stackalloc TPixel[pixelReader.PixelsPerRead];

    for (var i = 0;
         i < this.TileWidth * this.TileHeight;
         i += pixelReader.PixelsPerRead) {
      pixelIndexer.GetPixelCoordinates(i, out var x, out var y);
      var outOfBounds = xx + x >= imageWidth || yy + y >= imageHeight;

      if (outOfBounds) {
        pixelReader.Decode(br, junk, 0);
      } else {
        var dstOffs = (yy + y) * imageWidth + xx + x;
        pixelReader.Decode(br, scan0, dstOffs);
      }
    }
  }
}