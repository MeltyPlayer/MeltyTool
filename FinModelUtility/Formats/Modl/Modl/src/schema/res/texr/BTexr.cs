using fin.color;
using fin.data;
using fin.image;
using fin.image.formats;
using fin.image.io;
using fin.image.io.image;
using fin.image.io.pixel;
using fin.util.color;

using schema.binary;

using SixLabors.ImageSharp.PixelFormats;

namespace modl.schema.res.texr {
  public interface ITexr : IBinaryDeserializable {
    IImage Image { get; }
  }

  public abstract class BTexr {
    protected IImage ReadA8R8G8B8_(IBinaryReader br,
                                          uint width,
                                          uint height) {
      SectionHeaderUtil.AssertNameAndSize(
          br,
          "MIP ",
          width * height * 4);

      var image =
          new Rgba32Image(PixelFormat.RGBA8888, (int) width, (int) height);
      using var imageLock = image.Lock();
      var scan0 = imageLock.Pixels;

      var blockWidth = 4;
      var blockHeight = 4;

      var blockGrid = new Grid<(byte a, byte r)>(blockWidth, blockHeight);

      for (var blockY = 0; blockY < height / blockHeight; blockY++) {
        var availableBlockHeight =
            Math.Min(blockHeight, height - blockHeight * blockY);

        for (var blockX = 0; blockX < width / blockWidth; blockX++) {
          var availableBlockWidth =
              Math.Min(blockWidth, width - blockWidth * blockX);

          for (var iy = 0; iy < availableBlockHeight; ++iy) {
            for (var ix = 0; ix < availableBlockWidth; ++ix) {
              var a = br.ReadByte();
              var r = br.ReadByte();

              blockGrid[ix, iy] = (a, r);
            }
          }

          for (var iy = 0; iy < availableBlockHeight; ++iy) {
            var imgY = blockY * blockHeight + iy;
            for (var ix = 0; ix < availableBlockWidth; ++ix) {
              var imgX = blockX * blockWidth + ix;

              var (a, r) = blockGrid[ix, iy];
              var g = br.ReadByte();
              var b = br.ReadByte();

              scan0[(int) (imgY * width + imgX)] = new Rgba32(r, g, b, a);
            }
          }
        }
      }

      return image;
    }

    protected IImage ReadDxt1_(IBinaryReader br, uint width, uint height) {
      // TODO: Trim this little bit off?
      width = (uint) (MathF.Ceiling(width / 8f) * 8);
      height = (uint) (MathF.Ceiling(height / 8f) * 8);

      var mipSize = width * height >> 1;
      SectionHeaderUtil.AssertNameAndSize(br, "MIP ", mipSize);

      return new Dxt1aImageReader((int) width, (int) height).ReadImage(br);
    }

    protected IImage ReadP8_(IBinaryReader br, uint width, uint height) {
      SectionHeaderUtil.AssertNameAndSize(br, "PAL ", 512);

      var palette = br.ReadUInt16s(256)
                      .Select(value => ColorUtil.ParseRgb5A3(value))
                      .ToArray();

      SectionHeaderUtil.AssertNameAndSize(br, "MIP ", width * height);

      var indexImage = TiledImageReader
                       .New((int) width,
                            (int) height,
                            8,
                            4,
                            new L8PixelReader())
                       .ReadImage(br);

      return new IndexedImage8(PixelFormat.P8, indexImage, palette);
    }

    protected IImage ReadP4_(IBinaryReader br, uint width, uint height) {
      // TODO: This method seems incorrect...

      SectionHeaderUtil.AssertNameAndSize(br, "PAL ", 32);

      var palette = br.ReadUInt16s(16)
                      .Select(value => {
                        var r = ColorUtil.ExtractScaled(value, 0, 4);
                        var g = ColorUtil.ExtractScaled(value, 4, 4);
                        var b = ColorUtil.ExtractScaled(value, 8, 4);
                        var a = ColorUtil.ExtractScaled(value, 12, 4);

                        // TODO: Is this correct??? Textures don't look quite right
                        return FinColor.FromRgbaBytes(r, g, b, a);
                      })
                      .ToArray();

      SectionHeaderUtil.AssertNameAndSize(br, "MIP ", width * height / 2);

      var indexImage = TiledImageReader
                       .New((int) width,
                            (int) height,
                            8,
                            8,
                            new L4PixelReader())
                       .ReadImage(br);

      return new IndexedImage8(PixelFormat.P4, indexImage, palette);
    }

    protected IImage ReadIA8_(IBinaryReader br, uint width, uint height) {
      SectionHeaderUtil.AssertNameAndSize(br, "MIP ", 2 * width * height);
      return TiledImageReader
             .New((int) width, (int) height, 4, 4, new La16PixelReader())
             .ReadImage(br);
    }

    protected IImage ReadIA4_(IBinaryReader br, uint width, uint height) {
      SectionHeaderUtil.AssertNameAndSize(br, "MIP ", width * height);
      return TiledImageReader
             .New((int) width, (int) height, 8, 4, new La8PixelReader())
             .ReadImage(br);
    }

    protected IImage ReadI8_(IBinaryReader br, uint width, uint height) {
      SectionHeaderUtil.AssertNameAndSize(br, "MIP ", width * height);
      return TiledImageReader
             .New((int) width, (int) height, 8, 4, new L8PixelReader())
             .ReadImage(br.ReadBytes(width * height));
    }

    protected IImage ReadI4_(IBinaryReader br, uint width, uint height) {
      SectionHeaderUtil.AssertNameAndSize(br, "MIP ", width * height / 2);
      return TiledImageReader
             .New((int) width, (int) height, 8, 8, new L8PixelReader())
             .ReadImage(br);
    }
  }
}