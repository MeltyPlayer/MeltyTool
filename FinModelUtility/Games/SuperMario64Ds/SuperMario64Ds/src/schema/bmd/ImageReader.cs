using System.Drawing;
using System.Numerics;

using fin.image;
using fin.image.formats;
using fin.util.asserts;
using fin.util.color;

using schema.binary;

using SixLabors.ImageSharp.PixelFormats;

namespace sm64ds.schema.bmd;

/// <summary>
///   Shamelessly stolen from:
///   https://github.com/Arisotura/SM64DSe/blob/master/SM64DSFormats/BMD.NitroTexture.cs
/// </summary>
public class ImageReader {
  public static IImage ReadImage(Texture texture,
                                 Palette? palette) {
    switch (texture.TextureType) {
      case TextureType.A3_I5: break;
      case TextureType.PALETTE_4:
        return ReadPalette4_(texture, palette.AssertNonnull());
      case TextureType.PALETTE_16:
        return ReadPalette16_(texture, palette.AssertNonnull());
      case TextureType.PALETTE_256:
        return ReadPalette256_(texture, palette.AssertNonnull());
      case TextureType.TEX_4X4:
        return ReadTex4x4_(texture, palette.AssertNonnull());
      case TextureType.A5_I3:  break;
      case TextureType.DIRECT: break;
      default:                 throw new ArgumentOutOfRangeException();
    }

    return FinImage.Create1x1FromColor(Color.Red);
  }

  private static IImage ReadPalette4_(Texture texture, Palette palette) {
    var paletteColors = GetPaletteColors_(texture, palette);

    var image = new Rgba32Image(texture.Width, texture.Height);
    var fastLock = image.Lock();
    var dst = fastLock.Pixels;
    var dstI = 0;

    using var textureBr = new SchemaBinaryReader(texture.Data);
    while (!textureBr.Eof) {
      var texel = textureBr.ReadByte();

      for (var i = 0; i < 4; ++i) {
        var subTexel = (texel >> (2 * i)) & 0x3;
        dst[dstI++] = paletteColors[subTexel];
      }
    }

    return image;
  }

  private static IImage ReadPalette16_(Texture texture, Palette palette) {
    var paletteColors = GetPaletteColors_(texture, palette);

    var image = new Rgba32Image(texture.Width, texture.Height);
    var fastLock = image.Lock();
    var dst = fastLock.Pixels;
    var dstI = 0;

    using var textureBr = new SchemaBinaryReader(texture.Data);
    while (!textureBr.Eof) {
      var texel = textureBr.ReadByte();

      for (var i = 0; i < 2; ++i) {
        var subTexel = (texel >> (4 * i)) & 0xF;
        dst[dstI++] = paletteColors[subTexel];
      }
    }

    return image;
  }

  private static IImage ReadPalette256_(Texture texture, Palette palette) {
    var paletteColors = GetPaletteColors_(texture, palette);

    var image = new Rgba32Image(texture.Width, texture.Height);
    var fastLock = image.Lock();
    var dst = fastLock.Pixels;
    var dstI = 0;

    using var textureBr = new SchemaBinaryReader(texture.Data);
    while (!textureBr.Eof) {
      var texel = textureBr.ReadByte();
      dst[dstI++] = paletteColors[texel];
    }

    return image;
  }

  private static IImage ReadTex4x4_(Texture texture, Palette palette) {
    var paletteColors = GetPaletteColors_(texture, palette);

    var width = texture.Width;
    var image = new Rgba32Image(width, texture.Height);
    var fastLock = image.Lock();
    var dst = fastLock.Pixels;

    var blockSize = 4;
    var blockXCount = width / blockSize;

    var blockCount = texture.Data.Length / 6;
    using var textureBr = new SchemaBinaryReader(texture.Data);
    var blocks = textureBr.ReadUInt32s(blockCount);
    var palIndices = textureBr.ReadUInt16s(blockCount);

    foreach (var (i, (blox0, palidx_data)) in blocks.Zip(palIndices).Index()) {
      var blockX = i % blockXCount;
      var blockY = (i - blockX) / blockXCount;

      var blox = blox0;

      for (int subY = 0; subY < blockSize; subY++) {
        for (int subX = 0; subX < blockSize; subX++) {
          byte texel = (byte) (blox & 0x3);
          blox >>= 2;

          int pal_offset = (int) ((palidx_data & 0x3FFF) << 1);
          ushort color_mode = (ushort) (palidx_data >> 14);

          Rgba32? color = null;
          switch (texel) {
            case 0: color = paletteColors[pal_offset]; break;
            case 1: color = paletteColors[pal_offset + 1]; break;
            case 2: {
              switch (color_mode) {
                case 0:
                case 2: color = paletteColors[pal_offset + 2]; break;
                case 1: {
                  Rgba32 c0 = paletteColors[pal_offset];
                  Rgba32 c1 = paletteColors[pal_offset + 1];
                  color = MixColors(c0, 1, c1, 1);
                }
                  break;
                case 3: {
                  Rgba32 c0 = paletteColors[pal_offset];
                  Rgba32 c1 = paletteColors[pal_offset + 1];
                  color = MixColors(c0, 5, c1, 3);
                }
                  break;
              }
            }
              break;
            case 3: {
              switch (color_mode) {
                case 0:
                case 1: color = null; break;
                case 2:
                  color = paletteColors[pal_offset + 3]; break;
                case 3: {
                  Rgba32 c0 = paletteColors[pal_offset];
                  Rgba32 c1 = paletteColors[pal_offset + 1];
                  color = MixColors(c0, 3, c1, 5);
                }
                  break;
              }
            }
              break;
          }

          var y = (blockY * blockSize) + subY;
          var x = (blockX * blockSize) + subX;
          var dstI = y * width + x;
          dst[dstI] = color ?? default;
        }
      }
    }

    return image;
  }

  // TODO: Optimize this to use stackalloc instead
  public static Rgba32[] GetPaletteColors_(Texture texture, Palette palette) {
    using var paletteBr = new SchemaBinaryReader(palette.Data);

    var paletteColors = new Rgba32[palette.Data.Length >> 1];
    for (var i = 0; i < paletteColors.Length; ++i) {
      ColorUtil.SplitRgb5A1(paletteBr.ReadUInt16(),
                            out var b,
                            out var g,
                            out var r,
                            out _);
      var a = (byte) (texture.UseTransparentColor0 && i == 0 ? 0 : 0xFF);

      paletteColors[i] = new Rgba32(r, g, b, a);
    }

    return paletteColors;
  }

  public static Rgba32 MixColors(Rgba32 color1,
                                 float w1,
                                 Rgba32 color2,
                                 float w2) {
    var r1 = color1.R;
    var g1 = color1.G;
    var b1 = color1.B;
    var a1 = color1.A;

    var r2 = color2.R;
    var g2 = color2.G;
    var b2 = color2.B;
    var a2 = color2.A;

    var rf = ((r1 * w1) + (r2 * w2)) / (w1 + w2);
    var gf = ((g1 * w1) + (g2 * w2)) / (w1 + w2);
    var bf = ((b1 * w1) + (b2 * w2)) / (w1 + w2);
    var af = ((a1 * w1) + (a2 * w2)) / (w1 + w2);

    return new Rgba32((byte) rf, (byte) gf, (byte) bf, (byte) af);
  }
}