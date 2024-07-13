using fin.image;
using fin.schema;

using mod.image;

using schema.binary;
using schema.binary.attributes;

namespace mod.schema.mod {
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
      using var br = new SchemaBinaryReader(this.imageData, Endianness.BigEndian);

      var images = new IImage[this.MipmapCount];
      for (var i = 0; i < images.Length; ++i) {
        images[i]
            = new ModImageReader(this.width >> i, this.height >> i, this.format)
                .ReadImage(br);
      }

      return images;
    }

    public override string ToString()
      => $"{this.unknowns[0]}, {this.unknowns[1]}, {this.unknowns[2]}, {this.unknowns[3]}";
  }

  [BinarySchema]
  public partial class TextureAttributes : IBinaryConvertible {
    public ushort TextureImageIndex = 0;
    private readonly ushort padding_ = 0;

    [Unknown]
    public ushort Unk0;

    [Unknown]
    public ushort Unk1;

    [Unknown]
    public float WidthPercent = 0;
  }
}