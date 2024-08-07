using System;

using fin.image;
using fin.image.formats;
using fin.schema;
using fin.util.color;

using gx;
using gx.image;

using schema.binary;
using schema.binary.attributes;

using SixLabors.ImageSharp.PixelFormats;

namespace jsystem.schema.jutility.bti;

/// <summary>
///   Shamelessly stolen from Hack.io:
///   https://github.com/SuperHackio/Hack.io/blob/6b06dc5829bcde6f41f04673fdd544eeef37a5c3/Hack.io.J3D/J3DTexture.cs#L1421
/// </summary>
public enum JutTransparency : byte {
  /// <summary>
  /// No Transperancy
  /// </summary>
  OPAQUE = 0x00,

  /// <summary>
  /// Only allows fully Transperant pixels to be see through
  /// </summary>
  CUTOUT = 0x01,

  /// <summary>
  /// Allows Partial Transperancy. Also known as XLUCENT
  /// </summary>
  TRANSLUCENT = 0x02,

  /// <summary>
  /// Unknown
  /// </summary>
  SPECIAL = 0xCC
}

/// <summary>
///   BTI files define standalone textures. They are often used to replace
///   dummy textures.
/// </summary>
[BinarySchema]
[LocalPositions]
public partial class Bti : IBinaryConvertible {
  [IntegerFormat(SchemaIntegerType.BYTE)]
  public GxTextureFormat Format;

  public JutTransparency AlphaSetting;
  public ushort Width;
  public ushort Height;
  public GxWrapMode WrapS;
  public GxWrapMode WrapT;

  [IntegerFormat(SchemaIntegerType.BYTE)]
  public bool PalettesEnabled;

  [IntegerFormat(SchemaIntegerType.BYTE)]
  public GxPaletteFormat PaletteFormat;

  [WLengthOfSequence(nameof(palette))]
  public ushort NrPaletteEntries;

  public uint PaletteOffset;
  public uint BorderColor;
  public GX_MIN_TEXTURE_FILTER MinFilter;
  public GX_MAG_TEXTURE_FILTER MagFilter;

  [Unknown]
  public byte MinLodTimes8;

  public byte MaxLodTimes8;

  public byte NrMipMap;

  [Unknown]
  public byte Unknown5;

  public short LodBiasTimes100;

  [WPointerTo(nameof(Data))]
  public uint DataOffset;

  [RAtPosition(nameof(DataOffset))]
  [RSequenceLengthSource(nameof(CompressedBufferSize_))]
  public byte[] Data;

  [Skip]
  public Rgba32[] palette;

  [ReadLogic]
  private void ReadPalettes_(IBinaryReader br) {
    long position = br.Position;
    this.palette = new Rgba32[this.NrPaletteEntries];

    br.Position = this.PaletteOffset;
    for (var i = 0; i < this.NrPaletteEntries; ++i) {
      switch (this.PaletteFormat) {
        case GxPaletteFormat.PAL_A8_I8: {
          var alpha = br.ReadByte();
          var intensity = br.ReadByte();
          this.palette[i] =
              new Rgba32(intensity, intensity, intensity, alpha);
          break;
        }
        case GxPaletteFormat.PAL_R5_G6_B5: {
          ColorUtil.SplitRgb565(br.ReadUInt16(),
                                out var r,
                                out var b,
                                out var g);
          this.palette[i] = new Rgba32(r, g, b);
          break;
        }
        // TODO: There seems to be a bug reading the palette, these colors look weird
        case GxPaletteFormat.PAL_A3_RGB5: {
          ColorUtil.SplitRgb5A3(br.ReadUInt16(),
                                out var r,
                                out var g,
                                out var b,
                                out var a);
          this.palette[i] = new Rgba32(r, g, b, a);
          break;
        }
        default:
          throw new ArgumentOutOfRangeException();
      }
    }

    br.Position = position;
  }

  public IReadOnlyImage[] ToMipmapImages() {
    var mipmapImages = new IReadOnlyImage[this.NrMipMap];

    using var br = new SchemaBinaryReader(this.Data!, Endianness.BigEndian);

    if (this.Format != GxTextureFormat.INDEX4 &&
        this.Format != GxTextureFormat.INDEX8) {
      for (var i = 0; i < mipmapImages.Length; ++i) {
        mipmapImages[i]
            = new GxImageReader(this.Width >> i, this.Height >> i, this.Format)
                .ReadImage(br);
      }
    } else {
      var isIndex4 = this.Format == GxTextureFormat.INDEX4;

      for (var m = 0; m < mipmapImages.Length; ++m) {
        var width = this.Width >> m;
        var height = this.Height >> m;

        var bitmap = new Rgba32Image(isIndex4 ? PixelFormat.P4 : PixelFormat.P8,
                                     width,
                                     height);
        using var imageLock = bitmap.Lock();
        var ptr = imageLock.Pixels;

        var indices = new byte[width * height];
        if (isIndex4) {
          for (var i = 0; i < this.Data.Length; ++i) {
            var two = br.ReadByte();

            var firstIndex = two >> 4;
            var secondIndex = two & 0x0F;

            indices[2 * i + 0] = (byte) firstIndex;
            indices[2 * i + 1] = (byte) secondIndex;
          }
        } else {
          br.ReadBytes(indices);
        }

        var blockWidth = 8;
        var blockHeight = isIndex4 ? 8 : 4;

        var index = 0;
        for (var ty = 0; ty < height / blockHeight; ty++) {
          for (var tx = 0; tx < width / blockWidth; tx++) {
            for (var y = 0; y < blockHeight; ++y) {
              for (var x = 0; x < blockWidth; ++x) {
                ptr[(ty * blockHeight + y) * width + (tx * blockWidth + x)] =
                    this.palette[indices[index++]];
              }
            }
          }
        }

        mipmapImages[m] = bitmap;
      }
    }

    return mipmapImages;
  }

  [Skip]
  private int CompressedBufferSize_ {
    get {
      int num1 = (int) this.Width + (8 - (int) this.Width % 8) % 8;
      int num2 = (int) this.Width + (4 - (int) this.Width % 4) % 4;
      int num3 = (int) this.Height + (8 - (int) this.Height % 8) % 8;
      int num4 = (int) this.Height + (4 - (int) this.Height % 4) % 4;
      return this.Format switch {
          GxTextureFormat.I4         => num1 * num3 / 2,
          GxTextureFormat.I8         => num1 * num4,
          GxTextureFormat.A4_I4      => num1 * num4,
          GxTextureFormat.A8_I8      => num2 * num4 * 2,
          GxTextureFormat.R5_G6_B5   => num2 * num4 * 2,
          GxTextureFormat.A3_RGB5    => num2 * num4 * 2,
          GxTextureFormat.ARGB8      => num2 * num4 * 4,
          GxTextureFormat.INDEX4     => num1 * num3 / 2,
          GxTextureFormat.INDEX8     => num1 * num4,
          GxTextureFormat.INDEX14_X2 => num2 * num4 * 2,
          GxTextureFormat.S3TC1      => num2 * num4 / 2,
          _                          => -1
      };
    }
  }
}