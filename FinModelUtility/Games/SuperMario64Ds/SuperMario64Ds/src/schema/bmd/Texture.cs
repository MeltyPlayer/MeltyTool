using fin.math;

using schema.binary;
using schema.binary.attributes;

namespace sm64ds.schema.bmd;

public enum TextureType {
  A3_I5 = 1,
  PALETTE_4 = 2,
  PALETTE_16 = 3,
  PALETTE_256 = 4,
  TEX_4X4 = 5,
  A5_I3 = 6,
  DIRECT = 7
}

/// <summary>
///   Shamelessly stolen from:
///   https://kuribo64.net/get.php?id=KBNyhM0kmNiuUBb3
/// </summary>
[BinarySchema]
public partial class Texture : IBinaryConvertible {
  private uint nameOffset_;

  [NullTerminatedString]
  [RAtPosition(nameof(nameOffset_))]
  public string Name { get; set; }

  private uint dataOffset_;
  private uint rawDataLength_;

  public ushort Width { get; set; }
  public ushort Height { get; set; }

  public uint Parameters { get; set; }

  [Skip]
  public TextureType TextureType
    => (TextureType) this.Parameters.ExtractFromRight(26, 3);

  [Skip]
  public bool UseTransparentColor0 => this.Parameters.GetBit(29);

  [Skip]
  private uint TrueDataLength_ {
    get {
      if (this.TextureType == TextureType.TEX_4X4) {
        var blockCount = this.rawDataLength_ / 4;
        return blockCount * 4 + blockCount * 2;
      }

      return this.rawDataLength_;
    }
  }

  [RAtPosition(nameof(dataOffset_))]
  [RSequenceLengthSource(nameof(TrueDataLength_))]
  public byte[] Data { get; set; }
}