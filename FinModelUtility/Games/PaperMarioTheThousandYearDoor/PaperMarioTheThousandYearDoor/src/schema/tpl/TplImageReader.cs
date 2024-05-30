using fin.image;
using fin.image.io;
using fin.image.io.image;
using fin.image.io.pixel;

using schema.binary;

namespace ttyd.schema.tpl {
  public class TplImageReader : IImageReader {
    private readonly IImageReader impl_;

    public TplImageReader(int width, int height, TplImageFormat format) {
      this.impl_ = this.CreateImpl_(width, height, format);
    }

    public IImage ReadImage(IBinaryReader br) => this.impl_.ReadImage(br);

    private IImageReader CreateImpl_(int width,
                                     int height,
                                     TplImageFormat format) {
      return format switch {
          TplImageFormat.I4 => TiledImageReader.New(
              width,
              height,
              8,
              8,
              new L4PixelReader()),
          TplImageFormat.I8 => TiledImageReader.New(
              width,
              height,
              8,
              4,
              new L8PixelReader()),
          TplImageFormat.IA4 => TiledImageReader.New(
              width,
              height,
              8,
              4,
              new La8PixelReader()),
          TplImageFormat.IA8 => TiledImageReader.New(
              width,
              height,
              4,
              4,
              new La16PixelReader()),
          TplImageFormat.RGB565 => TiledImageReader.New(
              width,
              height,
              4,
              4,
              new Rgb565PixelReader()),
          TplImageFormat.RGB5A3 => TiledImageReader.New(
              width,
              height,
              4,
              4,
              new Rgba5553PixelReader()),
          TplImageFormat.RGBA8 => TiledImageReader.New(
              width,
              height,
              4,
              4,
              new Rgba32PixelReader()),
          TplImageFormat.CMPR => new Dxt1aImageReader(width, height),
      };
    }
  }
}