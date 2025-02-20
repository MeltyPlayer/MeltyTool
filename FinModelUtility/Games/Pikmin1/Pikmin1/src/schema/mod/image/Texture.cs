using System;

using fin.image;
using fin.schema;

using gx;
using gx.image;

using schema.binary;
using schema.binary.attributes;


namespace pikmin1.schema.mod {
  [BinarySchema]
  public partial class Texture : IBinaryConvertible {
    public enum TextureFormat : uint {
      RGB565 = 0,
      CMPR = 1,
      RGB5A3 = 2,
      I4 = 3,
      I8 = 4,
      IA4 = 5,
      IA8 = 6,
      RGBA32 = 7,
    }

    [Skip]
    public int index;

    [Skip]
    public string Name => "texture" + this.index + "_" + this.format;

    public ushort width = 0;
    public ushort height = 0;
    public TextureFormat format = 0;

    public uint MipmapCount;

    [Unknown]
    public readonly uint[] unknowns = new uint[4];

    [SequenceLengthSource(SchemaIntegerType.UINT32)]
    public byte[] imageData { get; set; }

    public IImage[] ToMipmapImages() {
      using var br
          = new SchemaBinaryReader(this.imageData, Endianness.BigEndian);

      var images = new IImage[this.MipmapCount];
      for (var i = 0; i < images.Length; ++i) {
        images[i] = new GxImageReader(
                this.width >> i,
                this.height >> i,
                this.format switch {
                    TextureFormat.RGB565 => GxTextureFormat.R5_G6_B5,
                    TextureFormat.CMPR => GxTextureFormat.S3TC1,
                    TextureFormat.RGB5A3 => GxTextureFormat.A3_RGB5,
                    TextureFormat.I4 => GxTextureFormat.I4,
                    TextureFormat.I8 => GxTextureFormat.I8,
                    TextureFormat.IA4 => GxTextureFormat.A4_I4,
                    TextureFormat.IA8 => GxTextureFormat.A8_I8,
                    TextureFormat.RGBA32 => GxTextureFormat.ARGB8,
                    _ => throw new ArgumentOutOfRangeException()
                })
            .ReadImage(br);
      }

      return images;
    }

    public override string ToString()
      => $"{this.unknowns[0]}, {this.unknowns[1]}, {this.unknowns[2]}, {this.unknowns[3]}";
  }

  [BinarySchema]
  public partial class TextureAttributes : IBinaryConvertible {
    public ushort TextureImageIndex { get; set; }
    private readonly ushort padding_ = 0;

    public ushort WrapFlags { get; set; }

    [Unknown]
    public ushort Unk1 { get; set; }

    [Unknown]
    public float WidthPercent { get; set; }
  }
}