using fin.image;
using fin.image.io;
using fin.image.io.pixel;
using fin.image.io.tile;

using gx;
using gx.image;

using schema.binary;

namespace jsystem.image;

public class J3dImageReader : IImageReader {
  private readonly IImageReader impl_;

  public J3dImageReader(int width, int height, GxTextureFormat format) {
      this.impl_ = this.CreateImpl_(width, height, format);
    }

  private IImageReader CreateImpl_(int width,
                                   int height,
                                   GxTextureFormat format) {
      return format switch {
          GxTextureFormat.I4 => TiledImageReader.New(
              width,
              height,
              8,
              8,
              new L2a4PixelReader()),
          GxTextureFormat.I8 => TiledImageReader.New(
              width,
              height,
              8,
              4,
              new L2a8PixelReader()),
          GxTextureFormat.A4_I4 => TiledImageReader.New(
              width,
              height,
              8,
              4,
              new Al8PixelReader()),
          GxTextureFormat.A8_I8 => TiledImageReader.New(
              width,
              height,
              4,
              4,
              new Al16PixelReader()),
          GxTextureFormat.R5_G6_B5 => TiledImageReader.New(
              width,
              height,
              4,
              4,
              new Rgb565PixelReader()),
          GxTextureFormat.A3_RGB5 => TiledImageReader.New(
              width,
              height,
              4,
              4,
              new Rgba5553PixelReader()),
          GxTextureFormat.ARGB8 => TiledImageReader.New(
              width,
              height,
              new Ar88Gb88TileReader()),
          GxTextureFormat.S3TC1 => new CmprImageReader(width, height),
      };
    }

  public IImage ReadImage(IBinaryReader br) => this.impl_.ReadImage(br);

  public IImage ReadImage(byte[] data, Endianness endianness)
    => this.impl_.ReadImage(data, endianness);
}