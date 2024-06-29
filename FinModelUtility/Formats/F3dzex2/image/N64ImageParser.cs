using System;
using System.Linq;

using fin.color;
using fin.image;
using fin.image.formats;
using fin.image.io;
using fin.image.io.pixel;
using fin.math;
using fin.util.color;

using schema.binary;

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
}

public class N64ImageParser {
  private readonly IN64Hardware n64Hardware_;

  public N64ImageParser(IN64Hardware n64Hardware) {
    this.n64Hardware_ = n64Hardware;
  }

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
    return Parse(colorFormat,
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
    var imageWidth = width;
    var imageHeight = height;

    switch (colorFormat) {
      case N64ColorFormat.RGBA: {
        switch (bitsPerTexel) {
          case BitsPerTexel._16BPT:
            return PixelImageReader.New(imageWidth,
                                        imageHeight,
                                        new Argb1555PixelReader())
                                   .ReadImage(data, Endianness.BigEndian);
          default:
            throw new ArgumentOutOfRangeException(
                nameof(bitsPerTexel),
                bitsPerTexel,
                null);
        }
      }
      case N64ColorFormat.L: {
        switch (bitsPerTexel) {
          case BitsPerTexel._4BPT:
            return PixelImageReader.New(imageWidth,
                                        imageHeight,
                                        new L4PixelReader())
                                   .ReadImage(data, Endianness.BigEndian);
          case BitsPerTexel._8BPT:
            return PixelImageReader.New(imageWidth,
                                        imageHeight,
                                        new L8PixelReader())
                                   .ReadImage(data, Endianness.BigEndian);
          default:
            throw new ArgumentOutOfRangeException(
                nameof(bitsPerTexel),
                bitsPerTexel,
                null);
        }
      }
      case N64ColorFormat.LA: {
        switch (bitsPerTexel) {
          case BitsPerTexel._8BPT:
            return PixelImageReader.New(imageWidth,
                                        imageHeight,
                                        new La8PixelReader())
                                   .ReadImage(data, Endianness.BigEndian);
          case BitsPerTexel._16BPT:
            return PixelImageReader.New(imageWidth,
                                        imageHeight,
                                        new La16PixelReader())
                                   .ReadImage(data, Endianness.BigEndian);
          default:
            throw new ArgumentOutOfRangeException(
                nameof(bitsPerTexel),
                bitsPerTexel,
                null);
        }
      }
      case N64ColorFormat.CI: {
        var indexedImage = bitsPerTexel switch {
            BitsPerTexel._4BPT => PixelImageReader
                                  .New(imageWidth,
                                       imageHeight,
                                       new L4PixelReader())
                                  .ReadImage(data, Endianness.BigEndian),
            BitsPerTexel._8BPT => PixelImageReader
                                  .New(imageWidth,
                                       imageHeight,
                                       new L8PixelReader())
                                  .ReadImage(data, Endianness.BigEndian),
            _ => throw new ArgumentOutOfRangeException(
                nameof(bitsPerTexel),
                bitsPerTexel,
                null)
        };

        uint maxIndex = 0;
        {
          using var imgLock = indexedImage.Lock();
          var ptr = imgLock.Pixels;
          for (var i = 0; i < width * height; ++i) {
            maxIndex = Math.Max(maxIndex, ptr[i].PackedValue);
          }
        }

        using var paletteEr =
            this.n64Hardware_.Memory.OpenAtSegmentedAddress(
                this.n64Hardware_.Rdp.PaletteSegmentedAddress);
        var palette = paletteEr
                      .ReadUInt16s(maxIndex + 1)
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

        return new IndexedImage8(bitsPerTexel switch {
                                     BitsPerTexel._4BPT => PixelFormat.P4,
                                     BitsPerTexel._8BPT => PixelFormat.P8,
                                 },
                                 indexedImage,
                                 palette);
      }
      default:
        throw new ArgumentOutOfRangeException(nameof(colorFormat),
                                              colorFormat,
                                              null);
    }
  }
}