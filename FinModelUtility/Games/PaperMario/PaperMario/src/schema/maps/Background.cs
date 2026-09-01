using schema.binary;
using schema.binary.attributes;

namespace pm.schema.maps;

/// <summary>
///   Shamelessly stolen from:
///   https://github.com/magcius/noclip.website/blob/main/src/PaperMario64/tex.ts#L175
/// </summary>
[BinarySchema]
public sealed partial class Background : IBinaryConvertible {
  private const uint BASE_RAM_ADDRESS_ = 0x80200000;

  public uint ImageRamAddress { get; set; }
  public uint PaletteRamAddress { get; set; }
  public uint Unk { get; set; }
  public ushort Width { get; set; }
  public ushort Height { get; set; }

  [Skip]
  public uint ImageOffset => this.ImageRamAddress - BASE_RAM_ADDRESS_;

  [Skip]
  public uint PaletteOffset => this.PaletteRamAddress - BASE_RAM_ADDRESS_;
}