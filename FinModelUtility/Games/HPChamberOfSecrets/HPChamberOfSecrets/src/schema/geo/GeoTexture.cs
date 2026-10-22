using fin.image;

using gx;
using gx.image;

using schema.binary;

namespace hpcos.schema.geo;

public enum GeoTextureFormat {
  RGB5A3 = 1,
  CMPR = 2,
  RGBA8 = 3,
}

public sealed class GeoTexture : IBinaryDeserializable {
  public const int SIZE = 0xbc;

  public long Offset { get; private set; }
  public int Width { get; private set; }
  public int Height { get; private set; }
  public GxWrapMode WrapModeU { get; private set; }
  public GxWrapMode WrapModeV { get; private set; }
  public string Name { get; private set; }
  public GeoTextureFormat Format { get; private set; }

  public long DataOffset { get; private set; }

  /// <summary>
  ///   Textures in unknown formats are kept so that texture indices still
  ///   line up, but can't be decoded.
  /// </summary>
  public bool IsSupported => Enum.IsDefined(this.Format);

  public void Read(IBinaryReader br) {
    this.Offset = br.Position;

    this.Width = br.ReadInt32At(this.Offset + 0x30);
    this.Height = br.ReadInt32At(this.Offset + 0x34);

    var wrapModes = br.ReadInt32At(this.Offset + 0x38);
    this.WrapModeU = (GxWrapMode) (((wrapModes ^ 2) >> 1) & 1);
    this.WrapModeV = (GxWrapMode) (((wrapModes ^ 4) >> 2) & 1);

    this.Name = br.ReadRelativeStringAt(this.Offset + 0x3c);
    this.Format = (GeoTextureFormat) br.ReadInt32At(this.Offset + 0x4c);
    this.DataOffset = br.ReadRelativePointerAt(this.Offset + 0x58);

    br.Position = this.Offset + SIZE;
  }

  /// <summary>
  ///   Reads and decodes the top mip level. Image data is only read on demand,
  ///   since most callers only need a few of a GEO's textures, if any.
  /// </summary>
  public IImage ToImage(IBinaryReader br) {
    // The length field is sometimes 0, so the size of the top mip level is
    // calculated from the dimensions instead.
    (GxTextureFormat gxFormat, int tileWidth, int tileHeight, int tileSize)
        = this.Format switch {
            GeoTextureFormat.RGB5A3 => (GxTextureFormat.A3_RGB5, 4, 4, 32),
            GeoTextureFormat.CMPR => (GxTextureFormat.S3TC1, 8, 8, 32),
            GeoTextureFormat.RGBA8 => (GxTextureFormat.ARGB8, 4, 4, 64),
            _ => throw new NotSupportedException(
                $"Unsupported GEO texture format: {this.Format}"),
        };
    int dataLength = (this.Width + tileWidth - 1) / tileWidth *
                     ((this.Height + tileHeight - 1) / tileHeight) *
                     tileSize;

    // Some textures run off the end of the file, so pad them with zeroes.
    byte[] imageData = new byte[dataLength];
    br.Position = this.DataOffset;
    int availableLength
        = (int) Math.Clamp(br.Length - this.DataOffset, 0, dataLength);
    br.ReadBytes(imageData.AsSpan(0, availableLength));

    return new GxImageReader(this.Width, this.Height, gxFormat).ReadImage(
        imageData,
        Endianness.BigEndian);
  }
}
