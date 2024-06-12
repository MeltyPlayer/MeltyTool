using fin.image;
using fin.image.io;
using fin.image.io.image;
using fin.image.io.pixel;

using mod.schema.mod;

using schema.binary;

namespace mod.image {
  public class ModImageReader : IImageReader {
    private readonly IImageReader impl_;

    public ModImageReader(int width, int height, Texture.TextureFormat format) {
      this.impl_ = this.CreateImpl_(width, height, format);
    }

    private IImageReader CreateImpl_(int width,
                                     int height,
                                     Texture.TextureFormat format) {
      return format switch {
          Texture.TextureFormat.I4 => TiledImageReader.New(
              width,
              height,
              8,
              8,
              new L4PixelReader()),
          Texture.TextureFormat.I8 => TiledImageReader.New(
              width,
              height,
              8,
              4,
              new L8PixelReader()),
          Texture.TextureFormat.IA4 => TiledImageReader.New(
              width,
              height,
              8,
              4,
              new La8PixelReader()),
          Texture.TextureFormat.IA8 => TiledImageReader.New(
              width,
              height,
              4,
              4,
              new La16PixelReader()),
          Texture.TextureFormat.RGB565 => TiledImageReader.New(
              width,
              height,
              4,
              4,
              new Rgb565PixelReader()),
          Texture.TextureFormat.RGB5A3 => TiledImageReader.New(
              width,
              height,
              4,
              4,
              new Rgba5553PixelReader()),
          Texture.TextureFormat.RGBA32 => TiledImageReader.New(
              width,
              height,
              4,
              4,
              new Rgba32PixelReader()),
          Texture.TextureFormat.CMPR => new Dxt1aImageReader(width, height),
      };
    }

    public IImage ReadImage(IBinaryReader br) => this.impl_.ReadImage(br);

    public IImage ReadImage(byte[] data, Endianness endianness)
      => this.impl_.ReadImage(data, endianness);
  }
}