using BCnEncoder.Decoder;
using BCnEncoder.Shared;

using fin.image;
using fin.image.formats;
using fin.io;

using schema.binary;

using SixLabors.ImageSharp.PixelFormats;

namespace xmod.api;

public class TexImageReader {
  public IImage ReadImage(IReadOnlyGenericFile texFile) {
      using var br = new SchemaBinaryReader(texFile.OpenRead());
      var width = br.ReadUInt16();
      var height = br.ReadUInt16();
      var dxtType = br.ReadUInt16();

      ColorRgba32[] loadedDxt;

      IImage image;
      PixelFormat pixelFormat;
      switch (dxtType) {
        // DXT1
        case 22: {
          pixelFormat = PixelFormat.DXT1;
          var expectedLength = width * height / 16 * (2 + 2 + 4);

          br.Position = 0xe;
          var bytes = br.ReadBytes(expectedLength);

          loadedDxt =
              new BcDecoder().DecodeRaw(bytes,
                                        width,
                                        height,
                                        CompressionFormat.Bc1);
          break;
        }
        // DXT3
        case 14: {
          pixelFormat = PixelFormat.DXT3;
          var expectedLength = width * height / 16 * (8 + 2 + 2 + 4);

          br.Position = 0xe;
          var bytes = br.ReadBytes(expectedLength);

          loadedDxt =
              new BcDecoder().DecodeRaw(bytes,
                                        width,
                                        height,
                                        CompressionFormat.Bc2);
          break;
        }
        // DXT5
        case 26: {
          pixelFormat = PixelFormat.DXT5;
          var expectedLength = width * height / 16 * (8 + 2 + 2 + 4);

          br.Position = 0xe;
          var bytes = br.ReadBytes(expectedLength);

          loadedDxt =
              new BcDecoder().DecodeRaw(bytes,
                                        width,
                                        height,
                                        CompressionFormat.Bc3);
          break;
        }
        default:
          throw new NotImplementedException();
      }

      var rgbaImage = new Rgba32Image(pixelFormat, width, height);
      image = rgbaImage;
      using var imageLock = rgbaImage.Lock();
      var ptr = imageLock.Pixels;

      for (var y = 0; y < height; y++) {
        for (var x = 0; x < width; ++x) {
          var i = y * width + x;

          var src = loadedDxt[i];
          ptr[i] = new Rgba32(src.r, src.g, src.b, src.a);
        }
      }

      return image;
    }
}