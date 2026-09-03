using f3dzex2.displaylist.opcodes;
using f3dzex2.image;

using fin.image;

using schema.binary;
using schema.binary.attributes;

namespace pm.schema.maps;

/// <summary>
///   Shamelessly stolen from:
///   https://github.com/magcius/noclip.website/blob/main/src/PaperMario64/tex.ts
/// </summary>
[BinarySchema]
public sealed partial class TextureArchive : IBinaryDeserializable {
  [RSequenceUntilEndOfStream]
  public TextureEnvironment[] TextureEnvironments { get; set; }
}

public enum ImageStorageType : byte {
  ONE,
  MIPMAPS,
  TWO_SAME_SETTINGS,
  TWO_DIFFERENT_SETTINGS,
}

[BinarySchema]
public sealed partial class TextureEnvironment : IBinaryDeserializable {
  [StringLengthSource(0x20)]
  public string Name { get; set; }

  public uint Attr0 { get; set; }
  public uint Attr1 { get; set; }
  public uint Attr2 { get; set; }
  public uint Attr3 { get; set; }

  [Skip]
  public ImageStorageType ImageStorageType
    => (ImageStorageType) ((this.Attr2 >>> 16) & 0xF);

  [Skip]
  public byte CombineMode => (byte) ((this.Attr2 >>> 8) & 0xFF);

  [Skip]
  public byte TexFilter => (byte) (this.Attr3 & 0xF);

  [Skip]
  public Image[] Images { get; set; }

  [ReadLogic]
  public void ReadImages_(IBinaryReader br) {
    switch (this.ImageStorageType) {
      case ImageStorageType.ONE: {
        this.Images = [
            new Image(br,
                      (ushort) this.Attr0,
                      (ushort) this.Attr1,
                      (ushort) this.Attr2,
                      (ushort) this.Attr3)
        ];
        break;
      }
      case ImageStorageType.MIPMAPS: {
        this.Images = [
            new Image(br,
                      (ushort) this.Attr0,
                      (ushort) this.Attr1,
                      (ushort) this.Attr2,
                      (ushort) this.Attr3)
        ];
        break;
      }
      case ImageStorageType.TWO_SAME_SETTINGS: break;
      case ImageStorageType.TWO_DIFFERENT_SETTINGS: break;
      default: throw new ArgumentOutOfRangeException();
    }
  }
}

public sealed class Image {
  public IImage Impl { get; set; }
  public F3dWrapMode WrapModeS { get; set; }
  public F3dWrapMode WrapModeT { get; set; }

  public Image(
      IBinaryReader br,
      ushort width,
      ushort height,
      ushort attr2,
      ushort attr3,
      int heightShift = 0) {
    height >>>= heightShift;

    var format = (N64ColorFormat) (attr2 & 0xF);
    var size = (BitsPerTexel) ((attr2 >>> 24) & 0xF);

    this.WrapModeS = (F3dWrapMode) ((attr3 >>> 16) & 0xF);
    this.WrapModeT = (F3dWrapMode) ((attr3 >>> 8) & 0xF);

    this.Impl = N64ImageParser.Parse(format, size, br, width, height);
  }
}