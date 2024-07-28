using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

using fin.color;
using fin.image;
using fin.image.formats;
using fin.io;
using fin.util.color;

using schema.binary;

using SixLabors.ImageSharp.PixelFormats;


// From https://github.com/mafaca/Dxt


namespace Dxt {
  public static class DxtDecoder {
    public enum CubeMapSide {
      POSITIVE_X,
      NEGATIVE_X,
      POSITIVE_Y,
      NEGATIVE_Y,
      POSITIVE_Z,
      NEGATIVE_Z,
    }

    public static (string, IDxt<IImage>) ReadDds(IReadOnlySystemFile ddsFile) {
      using var ddsStream = ddsFile.OpenRead();
      var br = new SchemaBinaryReader(ddsStream, Endianness.LittleEndian);
      br.AssertString("DDS ");
      br.AssertInt32(124); // size
      var flags = br.ReadInt32();

      var width = br.ReadInt32();
      var height = br.ReadInt32();

      var pitchOrLinearSize = br.ReadInt32();
      var depth = br.ReadInt32();
      // TODO: Read others
      var mipmapCount = br.ReadInt32();
      var reserved1 = br.ReadInt32s(11);

      // DDS_PIXELFORMAT
      br.AssertInt32(32); // size
      var pfFlags = br.ReadInt32();
      var pfFourCc = br.ReadString(4);
      var pfRgbBitCount = br.ReadInt32();
      var pfRBitMask = br.ReadInt32();
      var pfGBitMask = br.ReadInt32();
      var pfBBitMask = br.ReadInt32();
      var pfABitMask = br.ReadInt32();

      var caps1 = br.ReadInt32();

      var caps2 = br.ReadInt32();
      var isCubeMap = (caps2 & 0x200) != 0;
      var hasPositiveX = (caps2 & 0x400) != 0;
      var hasNegativeX = (caps2 & 0x800) != 0;
      var hasPositiveY = (caps2 & 0x1000) != 0;
      var hasNegativeY = (caps2 & 0x2000) != 0;
      var hasPositiveZ = (caps2 & 0x4000) != 0;
      var hasNegativeZ = (caps2 & 0x8000) != 0;
      var hasVolume = (caps2 & 0x200000) != 0;

      var sideCount = new[] {
          hasPositiveX,
          hasNegativeX,
          hasPositiveY,
          hasNegativeY,
          hasPositiveZ,
          hasNegativeZ
      }.Count(b => b);

      sideCount = Math.Max(1, sideCount);

      var queue = new Queue<CubeMapSide>();
      if (hasPositiveX) {
        queue.Enqueue(CubeMapSide.POSITIVE_X);
      }

      if (hasNegativeX) {
        queue.Enqueue(CubeMapSide.NEGATIVE_X);
      }

      if (hasPositiveY) {
        queue.Enqueue(CubeMapSide.POSITIVE_Y);
      }

      if (hasNegativeY) {
        queue.Enqueue(CubeMapSide.NEGATIVE_Y);
      }

      if (hasPositiveZ) {
        queue.Enqueue(CubeMapSide.POSITIVE_Z);
      }

      if (hasNegativeZ) {
        queue.Enqueue(CubeMapSide.NEGATIVE_Z);
      }

      br.Position = 128;

      switch (pfFourCc) {
        case "q\0\0\0": {
          var q000Text = "a16b16g16r16";

          var hdrCubeMap = new CubeMapImpl<IList<float>>();

          for (var s = 0; s < sideCount; s++) {
            var hdrMipMap = new MipMap<IList<float>>();

            for (var i = 0; i < mipmapCount; ++i) {
              var mmWidth = width >> i;
              var mmHeight = height >> i;

              var hdr = DecompressA16B16G16R16F(br, mmWidth, mmHeight);
              hdrMipMap.AddLevel(
                  new MipMapLevel<IList<float>>(hdr, mmWidth, mmHeight));
            }

            if (!isCubeMap) {
              return (
                  q000Text,
                  new DxtImpl<IImage>(
                      ToneMapAndConvertHdrMipMapsToBitmap(hdrMipMap)));
            }

            var side = queue.Dequeue();
            switch (side) {
              case CubeMapSide.POSITIVE_X: {
                hdrCubeMap.PositiveX = hdrMipMap;
                break;
              }
              case CubeMapSide.NEGATIVE_X: {
                hdrCubeMap.NegativeX = hdrMipMap;
                break;
              }
              case CubeMapSide.POSITIVE_Y: {
                hdrCubeMap.PositiveY = hdrMipMap;
                break;
              }
              case CubeMapSide.NEGATIVE_Y: {
                hdrCubeMap.NegativeY = hdrMipMap;
                break;
              }
              case CubeMapSide.POSITIVE_Z: {
                hdrCubeMap.PositiveZ = hdrMipMap;
                break;
              }
              case CubeMapSide.NEGATIVE_Z: {
                hdrCubeMap.NegativeZ = hdrMipMap;
                break;
              }
              default: throw new ArgumentOutOfRangeException();
            }
          }

          return (q000Text,
                  new DxtImpl<IImage>(
                      ToneMapAndConvertHdrCubemapToBitmap(hdrCubeMap)));
        }

        default: {
          ddsStream.Position = 0;
          return (pfFourCc,
                  new DxtImpl<IImage>(new DdsReader().Read(ddsStream)));
        }
      }
    }

    public static unsafe IList<float> DecompressA16B16G16R16F(
        SchemaBinaryReader br,
        int width,
        int height) {
      // Reads in the original HDR image. This IS NOT normalized to [0, 1].
      var hdr = new float[width * height * 4];

      var offset = 0;
      for (var y = 0; y < height; ++y) {
        for (var x = 0; x < width; ++x) {
          var r = br.ReadHalf();
          var g = br.ReadHalf();
          var b = br.ReadHalf();
          var a = br.ReadHalf();

          // TODO: This may be right, it sounds like this is what folks suggest online?
          r /= a;
          g /= a;
          b /= a;
          a /= a;

          // Processes gamma before tone-mapping below.
          hdr[offset++] = GammaToLinear(r);
          hdr[offset++] = GammaToLinear(g);
          hdr[offset++] = GammaToLinear(b);
          hdr[offset++] = GammaToLinear(a);
        }
      }

      return hdr;
    }

    public static IMipMap<IImage> ToneMapAndConvertHdrMipMapsToBitmap(
        IMipMap<IList<float>> hdrMipMap) {
      var max = -1f;
      foreach (var hdr in hdrMipMap) {
        max = MathF.Max(max, hdr.Impl.Max());
      }

      return ConvertHdrMipmapsToBitmap(hdrMipMap, max);
    }


    public static ICubeMap<IImage> ToneMapAndConvertHdrCubemapToBitmap(
        ICubeMap<IList<float>> hdrCubeMap) {
      // Tone-maps the HDR image so that it within [0, 1].
      // TODO: Is there a better algorithm than just the max?
      // TODO: Is this range available somewhere else, i.e. in the UGX file?
      var max = -1f;
      foreach (var hdrMipMap in hdrCubeMap) {
        foreach (var hdr in hdrMipMap) {
          max = MathF.Max(max, hdr.Impl.Max());
        }
      }

      var positiveX = hdrCubeMap.PositiveX != null
          ? ConvertHdrMipmapsToBitmap(
              hdrCubeMap.PositiveX,
              max)
          : null;
      var negativeX = hdrCubeMap.NegativeX != null
          ? ConvertHdrMipmapsToBitmap(
              hdrCubeMap.NegativeX,
              max)
          : null;

      var positiveY = hdrCubeMap.PositiveY != null
          ? ConvertHdrMipmapsToBitmap(
              hdrCubeMap.PositiveY,
              max)
          : null;
      var negativeY = hdrCubeMap.NegativeY != null
          ? ConvertHdrMipmapsToBitmap(
              hdrCubeMap.NegativeY,
              max)
          : null;

      var positiveZ = hdrCubeMap.PositiveZ != null
          ? ConvertHdrMipmapsToBitmap(
              hdrCubeMap.PositiveZ,
              max)
          : null;
      var negativeZ = hdrCubeMap.NegativeZ != null
          ? ConvertHdrMipmapsToBitmap(
              hdrCubeMap.NegativeZ,
              max)
          : null;

      return new CubeMapImpl<IImage> {
          PositiveX = positiveX,
          NegativeX = negativeX,
          PositiveY = positiveY,
          NegativeY = negativeY,
          PositiveZ = positiveZ,
          NegativeZ = negativeZ,
      };
    }

    private static IMipMap<IImage> ConvertHdrMipmapsToBitmap(
        IMipMap<IList<float>> hdrMipMap,
        float max)
      => new MipMap<IImage>(
          hdrMipMap.Select(
                       hdr => {
                         var width = hdr.Width;
                         var height = hdr.Height;
                         return (IMipMapLevel<IImage>) new
                             MipMapLevel<IImage>(
                                 ConvertHdrToBitmap(hdr.Impl,
                                   width,
                                   height,
                                   max),
                                 width,
                                 height);
                       })
                   .ToList());

    private static IImage ConvertHdrToBitmap(
        IList<float> hdr,
        int width,
        int height,
        float max) {
      var bitmap = new Rgba32Image(PixelFormat.RGBA16161616, width, height);
      using var imageLock = bitmap.Lock();
      var scan0 = imageLock.Pixels;

      var offset = 0;
      for (var y = 0; y < height; ++y) {
        for (var x = 0; x < width; ++x) {
          var r = hdr[offset + 0] / max * 255;
          var g = hdr[offset + 1] / max * 255;
          var b = hdr[offset + 2] / max * 255;

          // For some reason, alpha isn't used?
          // TODO: How is this factored in?
          var a = 255f; //hdrImage[offset + 3] / max * 255;

          scan0[y * width + x] =
              new Rgba32((byte) r, (byte) g, (byte) b, (byte) a);
        }
      }

      return bitmap;
    }

    private static float GammaToLinear(float gamma)
      => MathF.Pow(gamma, 1 / 2.2f);

    public static IImage DecompressDXT1(
        byte[] src,
        int srcOffset,
        int width,
        int height) {
      int offset = srcOffset;
      int bcw = (width + 3) / 4;
      int bch = (height + 3) / 4;
      int clen_last = (width + 3) % 4 + 1;

      Span<Rgb24> buffer = stackalloc Rgb24[16];
      Span<Rgb24> colors = stackalloc Rgb24[4];

      var bitmap = new Rgb24Image(PixelFormat.DXT1, width, height);
      using var imageLock = bitmap.Lock();
      var ptr = imageLock.Pixels;

      for (int t = 0; t < bch; t++) {
        for (int s = 0; s < bcw; s++, offset += 8) {
          var x = s * 4;

          byte r0, g0, b0, r1, g1, b1;
          ushort q0 = (ushort) (src[offset + 0] | src[offset + 1] << 8);
          ushort q1 = (ushort) (src[offset + 2] | src[offset + 3] << 8);
          Rgb565(q0, out r0, out g0, out b0);
          Rgb565(q1, out r1, out g1, out b1);
          colors[0] = new Rgb24(r0, g0, b0);
          colors[1] = new Rgb24(r1, g1, b1);
          if (q0 > q1) {
            colors[2] = new Rgb24((byte) ((r0 * 2 + r1) / 3),
                                  (byte) ((g0 * 2 + g1) / 3),
                                  (byte) ((b0 * 2 + b1) / 3));
            colors[3] = new Rgb24((byte) ((r0 + r1 * 2) / 3),
                                  (byte) ((g0 + g1 * 2) / 3),
                                  (byte) ((b0 + b1 * 2) / 3));
          } else {
            colors[2] = new Rgb24((byte) ((r0 + r1) / 2),
                                  (byte) ((g0 + g1) / 2),
                                  (byte) ((b0 + b1) / 2));
          }

          uint d = BitConverter.ToUInt32(src, offset + 4);
          var clen = (s < bcw - 1 ? 4 : clen_last);
          for (int i = 0, y = t * 4; i < 4 && y < height; i++, y++) {
            for (var c = 0; c < clen; ++c) {
              var colorIndex = (int) ((d >> (2 * (4 * i + c))) & 3);
              ptr[y * width + x + c] = colors[colorIndex];
            }
          }
        }
      }

      return bitmap;
    }


    public static IImage DecompressDXT3(
        byte[] input,
        int width,
        int height) {
      var image = new Rgba32Image(PixelFormat.DXT3, width, height);
      using var imageLock = image.Lock();
      var dstPtr = imageLock.Pixels;

      try {
        int bcw = width / 4;
        int bch = height / 4;

        int streamIndex = 0;
        // Remember where the alpha data is stored so we can decode simultaneously
        int alphaPtr = streamIndex;

        var dataIndex = 0;

        for (int t = 0; t < bch; t++) {
          for (int s = 0; s < bcw; s++) {
            // Shamelessly copied from:
            // https://github.com/nickbabcock/Pfim/blob/master/src/Pfim/dds/Dxt3Dds.cs

            /*
             * Strategy for decompression:
             * -We're going to decode both alpha and color at the same time
             * to save on space and time as we don't have to allocate an array
             * to store values for later use.
             */

            // Jump ahead to the color data
            streamIndex += 8;

            // Colors are stored in a pair of 16 bits
            ushort color0 = input[streamIndex++];
            color0 |= (ushort) (input[streamIndex++] << 8);

            ushort color1 = (input[streamIndex++]);
            color1 |= (ushort) (input[streamIndex++] << 8);

            // Extract R5G6B5
            var c0 = ColorUtil.ParseRgb565(color0);
            var c1 = ColorUtil.ParseRgb565(color1);

            Func<IColor, Rgb24> toRgb = c => new Rgb24(c.Rb, c.Gb, c.Bb);

            (var i0, var i1) = (toRgb(c0), toRgb(c1));
            Rgb24[] colors = [
                i0,
                i1,
                toRgb(ColorUtil.Interpolate(c0, c1, 1f / 3)),
                toRgb(ColorUtil.Interpolate(c0, c1, 2f / 3))
            ];

            for (int i = 0; i < 4; i++) {
              byte rowVal = input[streamIndex++];

              // Each row of rgb values have 4 alpha values that  are
              // encoded in 4 bits
              ushort rowAlpha = input[alphaPtr++];
              rowAlpha |= (ushort) (input[alphaPtr++] << 8);

              for (int j = 0; j < 8; j += 2) {
                byte currentAlpha = (byte) ((rowAlpha >> (j * 2)) & 0x0f);
                currentAlpha |= (byte) (currentAlpha << 4);
                var col = colors[((rowVal >> j) & 0x03)];
                dstPtr[dataIndex++] =
                    new Rgba32(col.R, col.G, col.B, currentAlpha);
              }
            }
          }
        }
      } catch (Exception e) {
        ;
      }


      return image;
    }


    /*public static void WriteToStreamFromRawDxt5a(
        Stream dst,
        byte[] src,
        int srcOffset,
        int width,
        int height) {
      var ew = new SchemaBinaryWriter(dst, Endianness.LittleEndian);

      var imageSize = width * height / 2;

      ew.Write("DDS ", Encoding.ASCII, false);
      ew.Write(124);
      ew.Write(0x000A1007);
      ew.Write(width);
      ew.Write(height);
      ew.Write(imageSize);
      ew.Write(0);
      ew.Write(1);

      for (var i = 0; i < 11; ++i) {
        ew.Write(0);
      }

      ew.Write(0x20);
      ew.Write(4);
      ew.Write("ATI1", Encoding.ASCII, false);
      ew.Write(0);
      ew.Write(0);
      ew.Write(0);
      ew.Write(0);
      ew.Write(0);
      ew.Write(0x401008);

      var fixedBuffer = new byte[imageSize];
      for (var i = 0; i < imageSize; i += 8) {
        fixedBuffer[i + 0] = src[srcOffset + i + 1];
        fixedBuffer[i + 1] = src[srcOffset + i + 0];

        fixedBuffer[i + 2] = src[srcOffset + i + 3];
        fixedBuffer[i + 3] = src[srcOffset + i + 2];
        fixedBuffer[i + 4] = src[srcOffset + i + 5];

        fixedBuffer[i + 5] = src[srcOffset + i + 4];
        fixedBuffer[i + 6] = src[srcOffset + i + 7];
        fixedBuffer[i + 7] = src[srcOffset + i + 6];
      }

      ew.Position = 128;
      ew.Write(fixedBuffer, 0, imageSize);
      Asserts.Equal(128 + imageSize, ew.Position);
    }*/

    public static IImage DecompressDxt5a(
        byte[] src,
        int srcOffset,
        int width,
        int height) {
      const int blockSize = 4;
      var blockCountX = width / blockSize;
      var blockCountY = height / blockSize;

      var imageSize = width * height / 2;

      var monoTable = new byte[8];
      var rIndices = new byte[16];

      var bitmap = new L8Image(PixelFormat.DXT5A, width, height);
      using var imageLock = bitmap.Lock();
      var ptr = imageLock.Pixels;

      for (var i = 0; i < imageSize; i += 8) {
        var iOff = srcOffset + i;

        // Gathers up color palette.
        var mono0 = monoTable[0] = src[iOff + 0];
        var mono1 = monoTable[1] = src[iOff + 1];

        var useEightIndexMode = mono0 > mono1;
        if (useEightIndexMode) {
          monoTable[2] = (byte) ((6 * mono0 + 1 * mono1) / 7f);
          monoTable[3] = (byte) ((5 * mono0 + 2 * mono1) / 7f);
          monoTable[4] = (byte) ((4 * mono0 + 3 * mono1) / 7f);
          monoTable[5] = (byte) ((3 * mono0 + 4 * mono1) / 7f);
          monoTable[6] = (byte) ((2 * mono0 + 5 * mono1) / 7f);
          monoTable[7] = (byte) ((1 * mono0 + 6 * mono1) / 7f);
        } else {
          monoTable[2] = (byte) ((4 * mono0 + 1 * mono1) / 5f);
          monoTable[3] = (byte) ((3 * mono0 + 2 * mono1) / 5f);
          monoTable[4] = (byte) ((2 * mono0 + 3 * mono1) / 5f);
          monoTable[5] = (byte) ((1 * mono0 + 4 * mono1) / 5f);
          monoTable[6] = 0;
          monoTable[7] = 255;
        }

        // Gathers up color indices.
        ulong indices = 0;
        for (var b = 0; b < 6; ++b) {
          ulong part = src[iOff + 2 + b];
          part <<= (8 * b);
          indices |= part;
        }

        for (var ii = 0; ii < 16; ++ii) {
          rIndices[ii] = (byte) (indices & 7);
          indices >>= 3;
        }

        // Writes pixels to output image.
        // TODO: This might actually be flipped across the X/Y axis. This is
        // kept this way to align with the albedo texture for now.
        var tileIndex = i / 8;
        var tileY = tileIndex % blockCountY;
        var tileX = (tileIndex - tileY) / blockCountX;

        for (var j = 0; j < blockSize; j++) {
          for (var k = 0; k < blockSize; k++) {
            var value = monoTable[rIndices[(j * blockSize) + k]];

            var outX = (tileX * blockSize) + j;
            var outY = tileY * blockSize + k;

            ptr[outY * width + outX] = new L8(value);
          }
        }
      }

      return bitmap;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void Rgb565(ushort c, out byte r, out byte g, out byte b) {
      var rI = (c & 0xf800) >> 8;
      var gI = (c & 0x07e0) >> 3;
      var bI = (c & 0x001f) << 3;
      r = (byte) (rI | (rI >> 5));
      g = (byte) (gI | (gI >> 6));
      b = (byte) (bI | (bI >> 5));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static int Color(int r, int g, int b, int a) {
      return r << 16 | g << 8 | b | a << 24;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void SplitColor_(int color,
                                    out byte r,
                                    out byte g,
                                    out byte b,
                                    out byte a) {
      r = (byte) ((color >> 16) & 0xff);
      g = (byte) ((color >> 8) & 0xff);
      b = (byte) (color & 0xff);
      a = (byte) ((color >> 24) & 0xff);
    }
  }
}