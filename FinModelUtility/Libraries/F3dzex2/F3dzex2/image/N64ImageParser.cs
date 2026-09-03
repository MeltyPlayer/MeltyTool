using System;
using System.Linq;

using fin.color;
using fin.image;
using fin.image.formats;
using fin.image.io;
using fin.image.io.pixel;
using fin.image.io.tile;
using fin.math;

using schema.binary;

using SixLabors.ImageSharp.PixelFormats;


namespace f3dzex2.image;

public enum N64ImageFormat : byte {
  // Note: "1 bit per pixel" is not a Fast3D format.
  _1BPP = 0x00,
  ARGB1555 = 0x10,
  RGBA8888 = 0x18,
  CI4 = 0x40,
  CI8 = 0x48,
  LA4 = 0x60,
  LA8 = 0x68,
  LA16 = 0x70,
  I4i = 0x80,
  I4ii = 0x90,
  I8 = 0x88,
}

public enum N64ColorFormat {
  RGBA = 0,
  YUV = 1,
  CI = 2,
  LA = 3,
  L = 4,
}

/// <summary>
///   I.e. bits per pixel.
/// </summary>
public enum BitsPerTexel {
  _4BPT = 0,
  _8BPT = 1,
  _16BPT = 2,
  _32BPT = 3,
}

public static class BitsPerTexelExtensions {
  public static int GetWordShift(this BitsPerTexel bitsPerTexel)
    => bitsPerTexel switch {
        BitsPerTexel._4BPT  => -1,
        BitsPerTexel._8BPT  => 0,
        BitsPerTexel._16BPT => 1,
        BitsPerTexel._32BPT => 2,
        _ => throw new ArgumentOutOfRangeException(
            nameof(bitsPerTexel),
            bitsPerTexel,
            null)
    };

  public static uint GetByteCount(this BitsPerTexel bitsPerTexel,
                                  uint texelCount)
    => ImageUtils.GetByteCount(texelCount, bitsPerTexel.GetBitCount());

  public static uint GetBitCount(this BitsPerTexel bitsPerTexel)
    => bitsPerTexel switch {
        BitsPerTexel._4BPT  => 4,
        BitsPerTexel._8BPT  => 8,
        BitsPerTexel._16BPT => 16,
        BitsPerTexel._32BPT => 32,
        _ => throw new ArgumentOutOfRangeException(
            nameof(bitsPerTexel),
            bitsPerTexel,
            null)
    };
}

public sealed class N64ImageParser(IN64Hardware n64Hardware) {
  public static void SplitN64ImageFormat(byte imageFormat,
                                         out N64ColorFormat colorFormat,
                                         out BitsPerTexel bitsPerTexel) {
    colorFormat =
        (N64ColorFormat) BitLogic.ExtractFromRight(imageFormat, 5, 3);
    bitsPerTexel =
        (BitsPerTexel) BitLogic.ExtractFromRight(imageFormat, 3, 2);
  }

  public IImage Parse(N64ImageFormat format,
                      byte[] data,
                      int width,
                      int height) {
    SplitN64ImageFormat((byte) format, out var colorFormat, out var bitSize);
    return this.Parse(colorFormat,
                      bitSize,
                      data,
                      width,
                      height);
  }

  public IImage Parse(N64ColorFormat colorFormat,
                      BitsPerTexel bitsPerTexel,
                      byte[] data,
                      int width,
                      int height) {
    var br = new SchemaBinaryReader(data, Endianness.BigEndian);
    return Parse(colorFormat, bitsPerTexel, br, n64Hardware, width, height);
  }

  public static IImage Parse(
      N64ColorFormat colorFormat,
      BitsPerTexel bitsPerTexel,
      IBinaryReader br,
      IN64Hardware n64Hardware,
      int width,
      int height) {
    var paletteBr = colorFormat == N64ColorFormat.CI
        ? n64Hardware.Memory.OpenAtSegmentedAddress(
            n64Hardware.Rdp.PaletteSegmentedAddress)
        : null;

    return Parse(colorFormat,
                 bitsPerTexel,
                 br,
                 paletteBr!,
                 width,
                 height,
                 n64Hardware.DeinterleaveImages);
  }

  public static IImage Parse(
      N64ColorFormat colorFormat,
      BitsPerTexel bitsPerTexel,
      IBinaryReader br,
      int width,
      int height,
      bool deinterleaveImages = false)
    => Parse(colorFormat,
             bitsPerTexel,
             br,
             br,
             width,
             height,
             deinterleaveImages);

  public static IImage Parse(
      N64ColorFormat colorFormat,
      BitsPerTexel bitsPerTexel,
      IBinaryReader imageBr,
      IBinaryReader paletteBr,
      int width,
      int height,
      bool deinterleaveImages = false) {
    var dst = new IImage[1];
    ParseMultiple(
        dst,
        colorFormat,
        bitsPerTexel,
        imageBr,
        paletteBr,
        width,
        height,
        false,
        deinterleaveImages);

    return dst[0];
  }

  public static void ParseMultiple(
      Span<IImage> images,
      N64ColorFormat colorFormat,
      BitsPerTexel bitsPerTexel,
      IBinaryReader br,
      int width,
      int height,
      bool mipmapping = false,
      bool deinterleaveImages = false)
    => ParseMultiple(
        images,
        colorFormat,
        bitsPerTexel,
        br,
        br,
        width,
        height,
        mipmapping,
        deinterleaveImages);

  public static void ParseMultiple(
      Span<IImage> images,
      N64ColorFormat colorFormat,
      BitsPerTexel bitsPerTexel,
      IBinaryReader imageBr,
      IBinaryReader paletteBr,
      int width,
      int height,
      bool mipmapping = false,
      bool deinterleaveImages = false) {
    var imageWidth = width;
    var imageHeight = height;

    IPixelIndexer pixelIndexer = deinterleaveImages
        ? new DeinterleavedPixelIndexer(width,
                                        bitsPerTexel switch {
                                            BitsPerTexel._4BPT  => 4,
                                            BitsPerTexel._8BPT  => 8,
                                            BitsPerTexel._16BPT => 16,
                                            BitsPerTexel._32BPT => 32,
                                        })
        : new BasicPixelIndexer(width);

    if (colorFormat != N64ColorFormat.CI) {
      for (var m = 0; m < images.Length; ++m) {
        if (mipmapping && m > 0) {
          imageWidth >>= 1;
          imageHeight >>= 1;
        }

        images[m] = colorFormat switch {
            N64ColorFormat.RGBA => bitsPerTexel switch {
                BitsPerTexel._16BPT => PixelImageReader.New(imageWidth,
                      imageHeight,
                      pixelIndexer,
                      new Argb1555PixelReader())
                    .ReadImage(imageBr),
                BitsPerTexel._32BPT => PixelImageReader.New(imageWidth,
                      imageHeight,
                      pixelIndexer,
                      new Argb32PixelReader())
                    .ReadImage(imageBr),
                _ => throw new ArgumentOutOfRangeException(
                    nameof(bitsPerTexel),
                    bitsPerTexel,
                    null)
            },
            N64ColorFormat.L => bitsPerTexel switch {
                BitsPerTexel._4BPT => PixelImageReader.New(imageWidth,
                      imageHeight,
                      pixelIndexer,
                      new I4PixelReader())
                    .ReadImage(imageBr),
                BitsPerTexel._8BPT => PixelImageReader.New(imageWidth,
                      imageHeight,
                      pixelIndexer,
                      new I8PixelReader())
                    .ReadImage(imageBr),
                _ => throw new ArgumentOutOfRangeException(
                    nameof(bitsPerTexel),
                    bitsPerTexel,
                    null)
            },
            N64ColorFormat.LA => bitsPerTexel switch {
                BitsPerTexel._4BPT => PixelImageReader.New(imageWidth,
                      imageHeight,
                      pixelIndexer,
                      new Al13PixelReader())
                    .ReadImage(imageBr),
                BitsPerTexel._8BPT => PixelImageReader.New(imageWidth,
                      imageHeight,
                      pixelIndexer,
                      new Al8PixelReader())
                    .ReadImage(imageBr),
                BitsPerTexel._16BPT => PixelImageReader.New(imageWidth,
                      imageHeight,
                      pixelIndexer,
                      new Al16PixelReader())
                    .ReadImage(imageBr),
                _ => throw new ArgumentOutOfRangeException(
                    nameof(bitsPerTexel),
                    bitsPerTexel,
                    null)
            },
            _ => throw new ArgumentOutOfRangeException(
                nameof(colorFormat),
                colorFormat,
                null)
        };
      }

      return;
    }

    var indexedImageMipmaps = new IImage<L8>[images.Length];
    for (var m = 0; m < indexedImageMipmaps.Length; ++m) {
      if (mipmapping && m > 0) {
        imageWidth >>= 1;
        imageHeight >>= 1;
      }

      indexedImageMipmaps[m] = bitsPerTexel switch {
          BitsPerTexel._4BPT => PixelImageReader.New(imageWidth,
                                                  imageHeight,
                                                  pixelIndexer,
                                                  new P4PixelReader())
                                                .ReadImage(imageBr),
          BitsPerTexel._8BPT => PixelImageReader
                                .New(imageWidth,
                                     imageHeight,
                                     pixelIndexer,
                                     new L8PixelReader())
                                .ReadImage(imageBr),
          _ => throw new ArgumentOutOfRangeException(
              nameof(bitsPerTexel),
              bitsPerTexel,
              null)
      };
    }

    var paletteLength = bitsPerTexel switch {
        BitsPerTexel._4BPT  => 0x10,
        BitsPerTexel._8BPT  => 0x100,
        _                   => throw new ArgumentOutOfRangeException(nameof(bitsPerTexel), bitsPerTexel, null)
    };

    paletteLength = (int) Math.Min(paletteLength, paletteBr.Length / 2);

    var palette = paletteBr
                  .ReadUInt16s(paletteLength)
                  .Select(value => {
                    ColorUtil.SplitArgb1555(
                        value,
                        out var r,
                        out var g,
                        out var b,
                        out var a);
                    return FinColor.FromRgbaBytes(r, g, b, a);
                  })
                  .ToArray();

    for (var m = 0; m < indexedImageMipmaps.Length; ++m) {
      var indexedImageMipmap = indexedImageMipmaps[m];
      images[m] = new IndexedImage8(
          bitsPerTexel switch {
              BitsPerTexel._4BPT => PixelFormat.P4,
              BitsPerTexel._8BPT => PixelFormat.P8,
              _ => throw new ArgumentOutOfRangeException(
                  nameof(bitsPerTexel),
                  bitsPerTexel,
                  null)
          },
          indexedImageMipmap,
          palette);
    }
  }
}