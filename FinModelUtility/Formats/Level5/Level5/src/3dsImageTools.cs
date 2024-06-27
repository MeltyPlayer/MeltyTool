using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace level5;

public class _3dsImageTools {
  public enum TexFormat {
    RGBA8 = 0x00,
    RGB8,
    RGBA5551,
    RGB565,
    RGBA4444,
    LA8,
    HILO8,
    L8,
    A8,
    LA4,
    L4,
    A4,
    ETC1,
    ETC1a4
  }

  public static readonly int[] zOrder_ = [
      0, 1, 4, 5, 16, 17, 20, 21,
      2, 3, 6, 7, 18, 19, 22, 23,
      8, 9, 12, 13, 24, 25, 28, 29,
      10, 11, 14, 15, 26, 27, 30, 31,
      32, 33, 36, 37, 48, 49, 52, 53,
      34, 35, 38, 39, 50, 51, 54, 55,
      40, 41, 44, 45, 56, 57, 60, 61,
      42, 43, 46, 47, 58, 59, 62, 63
  ];

  public static Bitmap DecodeImage(byte[] data, int width, int height, TexFormat type) {
      Bitmap bmp = new Bitmap(width, height, PixelFormat.Format32bppArgb);
      int[] pixels = new int[width * height];

      int p = 0;

      for (int h = 0; h < height; h += 8)
        for (int w = 0; w < width; w += 8) {
          // 8x8 block
          int[] colors = new int[64];
          for (int i = 0; i < 64; i++) {
            switch (type) {
              case TexFormat.RGBA8: colors[i] = Decode8888_(data[p++], data[p++], data[p++], data[p++]); break;
              case TexFormat.RGB8: colors[i] = Decode888_(data[p++], data[p++], data[p++]); break;
              case TexFormat.RGBA5551: colors[i] = Decode5551_(data[p++], data[p++]); break;
              case TexFormat.RGB565: colors[i] = Decode565(data[p++], data[p++]); break;
              case TexFormat.RGBA4444: colors[i] = Decode4444(data[p++], data[p++]); break;
              case TexFormat.LA8: colors[i] = DecodeLa8_(data[p++], data[p++]); break;
              case TexFormat.HILO8: colors[i] = DecodeHiLo8(data[p++], data[p++]); break;
              case TexFormat.L8: colors[i] = DecodeL8(data[p++]); break;
              case TexFormat.A8: colors[i] = DecodeA8(data[p++]); break;
              case TexFormat.LA4: colors[i] = DecodeLa4_(data[p++]); break;
              case TexFormat.L4: {
                  colors[i++] = DecodeL8((data[p] & 0xF) | ((data[p] & 0xF) << 4));
                  colors[i] = DecodeL8((data[p] & 0xF0) | ((data[p] & 0xF0) >> 4));
                  p++;
                  break;
                }
              case TexFormat.A4: {
                  colors[i++] = DecodeA8((data[p] & 0xF) | ((data[p] & 0xF) << 4));
                  colors[i] = DecodeA8((data[p] & 0xF0) | ((data[p] & 0xF0) >> 4));
                  p++;
                  break;
                }
              default: throw new Exception("Unsuppored format " + type.ToString("x"));
            }

          }

          for (int bh = 0; bh < 8; bh++)
            for (int bw = 0; bw < 8; bw++) {
              pixels[((w + bw) + (h + bh) * width)] = colors[CalcZOrder(bw, bh)];
            }
        }

      BitmapData bmpData = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.WriteOnly, bmp.PixelFormat);
      Marshal.Copy(pixels, 0, bmpData.Scan0, pixels.Length);
      bmp.UnlockBits(bmpData);

      return bmp;
    }

  public static byte[] EncodeImage(Bitmap img, TexFormat type) {
      //if (type == TexFormat.ETC1) return RG_ETC1.encodeETC(img);
      //if (type == TexFormat.ETC1a4) return RG_ETC1.encodeETCa4(img);

      List<byte> o = [];

      BitmapData bmpData = img.LockBits(new Rectangle(0, 0, img.Width, img.Height), ImageLockMode.WriteOnly, img.PixelFormat);
      int[] pixels = new int[img.Width * img.Height];
      Marshal.Copy(bmpData.Scan0, pixels, 0, pixels.Length);
      img.UnlockBits(bmpData);

      for (int h = 0; h < img.Height; h += 8)
        for (int w = 0; w < img.Width; w += 8) {
          // 8x8 block
          List<byte[]> colors = [];
          for (int bh = 0; bh < 8; bh++)
            for (int bw = 0; bw < 8; bw++) {
              switch (type) {
                case TexFormat.RGBA8: colors.Add(Encode8888(pixels[(w + bw) + (h + bh) * img.Width])); break;
                case TexFormat.RGB8: colors.Add(Encode8(pixels[(w + bw) + (h + bh) * img.Width])); break;
                case TexFormat.RGBA4444: colors.Add(Encode4444(pixels[(w + bw) + (h + bh) * img.Width])); break;
                case TexFormat.RGBA5551: colors.Add(encode5551(pixels[(w + bw) + (h + bh) * img.Width])); break;
                case TexFormat.RGB565: colors.Add(encode565(pixels[(w + bw) + (h + bh) * img.Width])); break;
                case TexFormat.LA8: colors.Add(encodeLA8(pixels[(w + bw) + (h + bh) * img.Width])); break;
                case TexFormat.HILO8: colors.Add(encodeHILO8(pixels[(w + bw) + (h + bh) * img.Width])); break;
                case TexFormat.L8: colors.Add(encodeL8(pixels[(w + bw) + (h + bh) * img.Width])); break;
                case TexFormat.A8: colors.Add(encodeA8(pixels[(w + bw) + (h + bh) * img.Width])); break;
                case TexFormat.LA4: colors.Add(encodeLA4(pixels[(w + bw) + (h + bh) * img.Width])); break;
                case TexFormat.L4: {
                    colors.Add([(byte)((encodeL8(pixels[(w + bw) + (h + bh) * img.Width])[0] / 0x11) & 0xF | ((encodeL8(pixels[(w + bw) + (h + bh) * img.Width + 1])[0] / 0x11) << 4))
                               ]);
                    bw++;
                    break;
                  }
                case TexFormat.A4: {
                    colors.Add([(byte)((encodeA8(pixels[(w + bw) + (h + bh) * img.Width])[0] / 0x11) & 0xF | ((encodeA8(pixels[(w + bw) + (h + bh) * img.Width + 1])[0] / 0x11) << 4))
                               ]);
                    bw++;
                    break;
                  }
              }

            }

          for (int bh = 0; bh < 8; bh++)
            for (int bw = 0; bw < 8; bw++) {
              int pos = bw + bh * 8;
              for (int i = 0; i < zOrder_.Length; i++)
                if (zOrder_[i] == pos) {
                  if (type == TexFormat.L4 || type == TexFormat.A4) { i /= 2; bw++; }
                  o.AddRange(colors[i]);
                  break;
                }
            }
        }

      return o.ToArray();
    }

  private static readonly int[] shift_ = [0x00, 0x01, 0x04, 0x05, 0x10, 0x11, 0x14, 0x15
  ];
  public static int CalcZOrder(int xPos, int yPos) {
      int x = shift_[xPos];
      int y = shift_[yPos] << 1;

      return x | y;
    }

  #region Decoding

  private static int Decode8888_(int b1, int b2, int b3, int b4) {
      return (b1 << 24) | (b4 << 16) | (b3 << 8) | b2;
    }

  private static int Decode888_(int b1, int b2, int b3) {
      return (255 << 24) | (b3 << 16) | (b2 << 8) | b1;
    }

  private static int Decode5551_(int b1, int b2) {
      int bt = b1 | (b2 << 8);
      int fst = (bt & 0xF800) >> 8;
      int scn = (bt & 0x07C0) >> 3;
      int thd = (bt & 0x003E) << 2;
      int a = (bt & 0x0001) >> 0;

      return (a * 255 << 24) | (fst << 16) | (scn << 8) | (thd);
    }

  private static int Decode565(int b1, int b2) {
      int bt = (b2 << 8) | b1;

      int a = 255;
      int r = (bt >> 11) & 0x1F;
      int g = (bt >> 5) & 0x3F;
      int b = (bt) & 0x1F;

      r = (r << 3) | (r >> 2);
      g = (g << 2) | (g >> 4);
      b = (b << 3) | (b >> 2);

      return (a << 24) | (r << 16) | (g << 8) | b;
    }

  public static int Decode4444(int b1, int b2) {
      int a = (b1 & 0x0F) * 17;
      int r = (b2 & 0xF0);
      int g = (b2 & 0x0F) * 17;
      int b = (b1 & 0xF0);

      a = a | ((a) >> 4);
      r = r | ((r) >> 4);
      g = g | ((g) >> 4);
      b = b | ((b) >> 4);

      return (a << 24) | (r << 16) | (g << 8) | b;
    }

  public static int DecodeL8(int b1) {
      return (255 << 24) | (b1 << 16) | (b1 << 8) | b1;
    }

  public static int DecodeA8(int b1) {
      return (b1 << 24) | (255 << 16) | (255 << 8) | 255;
    }

  private static int DecodeLa8_(int b1, int b2) {
      return (b1 << 24) | (b2 << 16) | (b2 << 8) | b2;
    }

  private static int DecodeLa4_(int b) {
      int r = b >> 4;
      int a = b & 0x0F;
      a = a | (a << 4);
      r = r | (r << 4);
      return (a << 24) | (r << 16) | (r << 8) | r;
    }

  public static int DecodeHiLo8(int b1, int b2) {
      return (255 << 24) | (b2 << 16) | (b1 << 8) | 255;
    }

  #endregion

  #region Encoding
  public static byte[] Encode8888(int color) {
      return [(byte)((color >> 24) & 0xFF), (byte)((color) & 0xFF), (byte)((color >> 8) & 0xFF), (byte)((color >> 16) & 0xFF)
      ];
    }

  public static byte[] Encode8(int color) {
      return [(byte)((color) & 0xFF), (byte)((color >> 8) & 0xFF), (byte)((color >> 16) & 0xFF)
      ];
    }

  public static byte[] Encode4444(int color) {
      int val = 0;
      val += (((color >> 24) & 0xFF) / 0x11);
      val += ((((color) & 0xFF) / 0x11) << 4);
      val += ((((color >> 8) & 0xFF) / 0x11) << 8);
      val += ((((color >> 16) & 0xFF) / 0x11) << 12);
      return [(byte)(val & 0xFF), (byte)(val >> 8)];
    }

  public static byte[] encodeA8(int color) {
      return [(byte)((color >> 24) & 0xFF)];
    }

  public static byte[] encodeL8(int color) {
      return [(byte)(((0x4CB2 * (color & 0xFF) + 0x9691 * ((color >> 8) & 0xFF) + 0x1D3E * ((color >> 8) & 0xFF)) >> 16) & 0xFF)
      ];
    }

  public static byte calLum(int color) {
      return (byte)(((0x4CB2 * (color & 0xFF) + 0x9691 * ((color >> 8) & 0xFF) + 0x1D3E * ((color >> 8) & 0xFF)) >> 16) & 0xFF);
    }

  public static byte[] encodeLA4(int color) {
      return [(byte)((((color >> 24) / 0x11) & 0xF | ((color >> 16) / 0x11) & 0xF << 4))
      ];
    }

  public static byte[] encodeLA8(int color) {
      return [(byte)((color >> 24) & 0xFF), (byte)((color >> 16) & 0xFF)];
    }

  public static byte[] encodeHILO8(int color) {
      return [(byte)((color) & 0xFF), (byte)((color >> 8) & 0xFF)];
    }

  public static byte[] encode565(int c) {
      int r = ((c >> 16) & 0xFF) >> 3;
      int g = ((c >> 8) & 0xFF) >> 2;
      int b = ((c) & 0xFF) >> 3;
      int val = (r << 11) | (g << 5) | b;
      return [(byte)(val & 0xFF), (byte)(val >> 8)];
    }

  public static byte[] encode5551(int c) {
      int val = 0;
      val += (byte)(((c >> 24) & 0xFF) > 0x80 ? 1 : 0);
      val += convert8to5(((c >> 16) & 0xFF)) << 11;
      val += convert8to5(((c >> 8) & 0xFF)) << 6;
      val += convert8to5(((c) & 0xFF)) << 1;
      ushort v = (ushort)val;
      return [(byte)(val & 0xFF), (byte)(val >> 8)];
    }

  #endregion

  static byte convert8to5(int col) {
      byte[] Convert8to5 = [
          0x00,0x08,0x10,0x18,0x20,0x29,0x31,0x39,
                                   0x41,0x4A,0x52,0x5A,0x62,0x6A,0x73,0x7B,
                                   0x83,0x8B,0x94,0x9C,0xA4,0xAC,0xB4,0xBD,
                                   0xC5,0xCD,0xD5,0xDE,0xE6,0xEE,0xF6,0xFF
      ];
      byte i = 0;
      while (col > Convert8to5[i]) i++;
      return i;
    }
}