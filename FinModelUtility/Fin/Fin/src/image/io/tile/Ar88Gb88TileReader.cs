using System;

using fin.image.formats;

using schema.binary;

using SixLabors.ImageSharp.PixelFormats;

namespace fin.image.io.tile;

public readonly struct Ar88Gb88TileReader : ITileReader<Rgba32> {
  public IImage<Rgba32> CreateImage(int width, int height)
    => new Rgba32Image(PixelFormat.AR88GB88, width, height);

  public int TileWidth => 4;
  public int TileHeight => 4;

  public void Decode(IBinaryReader br,
                     Span<Rgba32> scan0,
                     int tileX,
                     int tileY,
                     int imageWidth,
                     int imageHeight) {
    var x = tileX * this.TileWidth;
    var y = tileY * this.TileHeight;

    for (int k = 0; k < 2; k++) {
      for (int y1 = y; y1 < y + this.TileHeight; y1++) {
        for (int x1 = x; x1 < x + this.TileWidth; x1++) {
          var offset = y1 * imageWidth + x1;

          var pixel = br.ReadUInt16();

          if (x1 >= imageWidth || y1 >= imageHeight) {
            continue;
          }

          var rgba = scan0[offset];
          if (k == 0) {
            rgba.A = (byte) (pixel >> 8);
            rgba.R = (byte) (pixel & 0xff);
          } else {
            rgba.G = (byte) (pixel >> 8);
            rgba.B = (byte) (pixel & 0xff);
          }

          scan0[offset] = rgba;
        }
      }
    }
  }
}