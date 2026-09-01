using schema.binary;
using schema.binary.attributes;

namespace pm.schema.maps;

/// <summary>
///   Shamelessly stolen from:
///   https://github.com/magcius/noclip.website/blob/main/src/PaperMario64/map_shape.ts#L64
/// </summary>
[BinarySchema]
public sealed partial class Shape : IBinaryConvertible {
  private const uint BASE_RAM_ADDRESS_ = 0x80210000;

  public uint ModelTreeRootRamAddress { get; set; }
  public uint VertexTableRamAddress { get; set; }
  public uint ModelNameTableRamAddress { get; set; }
  public uint ColliderNameTableRamAddress { get; set; }
  public uint ZoneNameTableRamAddress { get; set; }

  [Skip]
  public uint ModelTreeRootOffset
    => this.ModelTreeRootRamAddress - BASE_RAM_ADDRESS_;


}