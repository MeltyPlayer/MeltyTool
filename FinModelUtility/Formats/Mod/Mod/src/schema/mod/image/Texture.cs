using fin.image;
using fin.schema;

using mod.image;

using schema.binary;
using schema.binary.attributes;

namespace mod.schema.mod;

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

  [Unknown]
  public readonly uint[] unknowns = new uint[5];

  [SequenceLengthSource(SchemaIntegerType.UINT32)]
  public byte[] imageData { get; set; }

  public IImage ToImage() {
      return new ModImageReader(this.width, this.height, this.format).ReadImage(
          this.imageData, Endianness.BigEndian);
    }
}

public enum TilingMode : byte {
  REPEAT = 0,
  CLAMP = 1,
  MIRROR_REPEAT = 2,
}

[BinarySchema]
public partial class TextureAttributes : IBinaryConvertible {
  public ushort index = 0;
  private readonly ushort padding_ = 0;
  public TilingMode TilingModeS { get; set; }
  public TilingMode TilingModeT { get; set; }

  [Unknown]
  public ushort unknown1 = 0;

  [Unknown]
  public float unknown2 = 0;
}