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
  public (Image, Image?) Images { get; set; }

  [ReadLogic]
  public void ReadImages_(IBinaryReader br) {
    switch (this.ImageStorageType) {
      case ImageStorageType.ONE: {
        this.Images = (
            Image.CreateOneFromAttrs(
                br,
                (ushort) this.Attr0,
                (ushort) this.Attr1,
                this.Attr2,
                this.Attr3),
            null
        );
        break;
      }
      case ImageStorageType.MIPMAPS: {
        var mipmapCount = 0;

        var size = (BitsPerTexel) ((this.Attr2 >>> 24) & 0xF);
        var widthCap = 32 >>> (int) size;
        var widthIter = (ushort) this.Attr0;
        while (true) {
          if (widthIter < widthCap) {
            break;
          }

          widthIter >>= 1;
        }

        this.Images = (
            Image.CreateOneFromAttrs(
                br,
                (ushort) this.Attr0,
                (ushort) this.Attr1,
                this.Attr2,
                this.Attr3,
                mipmapCount),
            null
        );
        break;
      }
      case ImageStorageType.TWO_SAME_SETTINGS: {
        this.Images = Image.CreateTwoFromAttrs(
            br,
            (ushort) this.Attr0,
            (ushort) this.Attr1,
            this.Attr2,
            this.Attr3);
        break;
      }
      case ImageStorageType.TWO_DIFFERENT_SETTINGS: {
        this.Images = (
            Image.CreateOneFromAttrs(
                br,
                (ushort) this.Attr0,
                (ushort) this.Attr1,
                this.Attr2,
                this.Attr3),
            Image.CreateOneFromAttrs(
                br,
                (ushort) (this.Attr0 >>> 16),
                (ushort) (this.Attr1 >>> 16),
                this.Attr2 >>> 4,
                this.Attr3 >>> 4)
            );
        break;
      }
      default: throw new ArgumentOutOfRangeException();
    }
  }
}

public sealed class Image {
  public required IImage[] Mipmaps { get; init; }
  public required F3dWrapMode WrapModeS { get; init; }
  public required F3dWrapMode WrapModeT { get; init; }

  public static Image CreateOneFromAttrs(
      IBinaryReader br,
      ushort width,
      ushort height,
      uint attr2,
      uint attr3,
      int mipmapCount = 1) {
    var (format, size, wrapModeS, wrapModeT) = SplitAttrs_(attr2, attr3);

    var mipmaps = new IImage[mipmapCount];
    N64ImageParser.ParseMultiple(mipmaps, format, size, br, width, height);

    return new Image {
        Mipmaps = mipmaps,
        WrapModeS = wrapModeS,
        WrapModeT = wrapModeT,
    };
  }

  public static (Image, Image) CreateTwoFromAttrs(
      IBinaryReader br,
      ushort width,
      ushort height,
      uint attr2,
      uint attr3) {
    height >>>= 1;

    var (format, size, wrapModeS, wrapModeT) = SplitAttrs_(attr2, attr3);

    var images = new IImage[2];
    N64ImageParser.ParseMultiple(images, format, size, br, width, height);

    return (new Image {
        Mipmaps = [images[0]],
        WrapModeS = wrapModeS,
        WrapModeT = wrapModeT,
    }, new Image {
        Mipmaps = [images[1]],
        WrapModeS = wrapModeS,
        WrapModeT = wrapModeT,
    });
  }

  private static (N64ColorFormat format, BitsPerTexel size, F3dWrapMode
      wrapModeS, F3dWrapMode wrapModeT)
      SplitAttrs_(uint attr2, uint attr3) {
    var format = (N64ColorFormat) (attr2 & 0xF);
    var size = (BitsPerTexel) ((attr3 >>> 24) & 0xF);

    var wrapModeS = (F3dWrapMode) ((attr3 >>> 16) & 0xF);
    var wrapModeT = (F3dWrapMode) ((attr3 >>> 8) & 0xF);

    return (format, size, wrapModeS, wrapModeT);
  }

  private Image() { }
}