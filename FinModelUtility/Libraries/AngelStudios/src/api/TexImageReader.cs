using System.Drawing;

using BCnEncoder.Decoder;
using BCnEncoder.Shared;

using CommunityToolkit.HighPerformance;

using fin.image;
using fin.image.formats;
using fin.image.io;
using fin.image.io.pixel;
using fin.io;

using schema.binary;

using SixLabors.ImageSharp.PixelFormats;


namespace xmod.api;

public class TexImageReader {
  public IImage ReadImage(IReadOnlyGenericFile texFile) {
    using var br = new SchemaBinaryReader(texFile.OpenRead());
    var width = br.ReadUInt16();
    var height = br.ReadUInt16();
    var imageType = br.ReadUInt16();
    var mipmapCount = br.ReadUInt16();
    var unk = br.ReadUInt16();
    var flags = br.ReadUInt32();

    // Tries to read the simple formats first.
    switch (imageType) {
      case 18:
        return PixelImageReader.New(width, height, new Rgba32PixelReader())
                               .ReadImage(br);
    }

    // Otherwise, this is DXT.
    ColorRgba32[] loadedDxt;
    IImage image;
    PixelFormat pixelFormat;
    switch (imageType) {
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
        return FinImage.Create1x1FromColor(Color.Magenta);
    }

    var rgbaImage = new Rgba32Image(pixelFormat, width, height);
    image = rgbaImage;
    using var imageLock = rgbaImage.Lock();
    loadedDxt.AsSpan().Cast<ColorRgba32, Rgba32>().CopyTo(imageLock.Pixels);

    return image;
  }
}