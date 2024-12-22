using System.Drawing;

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
  public static IImage ReadImage(Texture texture, Palette? palette) {
    switch (texture.TextureType) {
      case TextureType.A3_I5:      break;
      case TextureType.PALETTE_4:
        return ReadPalette4_(texture, palette.AssertNonnull());
      case TextureType.PALETTE_16:
        return ReadPalette16_(texture, palette.AssertNonnull());
      case TextureType.PALETTE_256:
        return ReadPalette256_(texture, palette.AssertNonnull());
      case TextureType.TEX_4X4: break;
      case TextureType.A5_I3:   break;
      case TextureType.DIRECT:  break;
      default:                  throw new ArgumentOutOfRangeException();
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

  /*private static IImage ReadTex4x4_(Texture texture, Palette palette) {
    var paletteColors = GetPaletteColors_(texture, palette);

    int yOut = 0, xOut = 0;

    var width = texture.Width;
    var image = new Rgba32Image(width, texture.Height);
    var fastLock = image.Lock();
    var dst = fastLock.Pixels;

    using var textureBr = new SchemaBinaryReader(texture.Data);

    while (!textureBr.Eof) {
      uint blox = Helper.BytesToUInt32(tex, _in);
      ushort palidx_data = texture.
          = Helper.BytesToUShort16(tex, m_TextureDataLength + (_in >> 1));

      for (int y = 0; y < 4; y++) {
        for (int x = 0; x < 4; x++) {
          byte texel = (byte) (blox & 0x3);
          blox >>= 2;

          int pal_offset = (int) ((palidx_data & 0x3FFF) << 2);
          ushort color_mode = (ushort) (palidx_data >> 14);
          uint color = 0xFFFFFFFF;

          switch (texel) {
            case 0: color = Helper.BytesToUShort16(pal, pal_offset, 0); break;
            case 1:
              color = Helper.BytesToUShort16(pal, pal_offset + 2, 0); break;
            case 2: {
              switch (color_mode) {
                case 0:
                case 2:
                  color = Helper.BytesToUShort16(pal, pal_offset + 4, 0); break;
                case 1: {
                  ushort c0 = Helper.BytesToUShort16(pal, pal_offset, 0);
                  ushort c1 = Helper.BytesToUShort16(pal, pal_offset + 2, 0);
                  color = Helper.BlendColorsBGR15(c0, 1, c1, 1);
                }
                  break;
                case 3: {
                  ushort c0 = Helper.BytesToUShort16(pal, pal_offset, 0);
                  ushort c1 = Helper.BytesToUShort16(pal, pal_offset + 2, 0);
                  color = Helper.BlendColorsBGR15(c0, 5, c1, 3);
                }
                  break;
              }
            }
              break;
            case 3: {
              switch (color_mode) {
                case 0:
                case 1: color = 0xFFFFFFFF; break;
                case 2:
                  color = Helper.BytesToUShort16(pal, pal_offset + 6, 0); break;
                case 3: {
                  ushort c0 = Helper.BytesToUShort16(pal, pal_offset, 0);
                  ushort c1 = Helper.BytesToUShort16(pal, pal_offset + 2, 0);
                  color = Helper.BlendColorsBGR15(c0, 3, c1, 5);
                }
                  break;
              }
            }
              break;
          }

          int _out = (int) (((y * width) + x) * 4);
          int yoff = (int) (y * width * 4);
          int xoff = (int) (x * 4);

          var dstI = _out + yoff + xoff;

          if (color == 0xFFFFFFFF) {
            dst[dstI] = default;
          } else {
            dst[dstI] = ;
            byte red = (byte) ((color & 0x001F) << 3);
            byte green = (byte) ((color & 0x03E0) >> 2);
            byte blue = (byte) ((color & 0x7C00) >> 7);

            m_ARGB[_out + yoff + xoff] = blue;
            m_ARGB[_out + yoff + xoff + 1] = green;
            m_ARGB[_out + yoff + xoff + 2] = red;
            m_ARGB[_out + yoff + xoff + 3] = 0xFF;
          }
        }
      }

      xOut += 4;
      if (xOut >= width) {
        xOut = 0;
        yOut += 4;
      }
    }

    return image;
  }*/

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
}