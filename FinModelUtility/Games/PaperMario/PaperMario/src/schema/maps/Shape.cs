using schema.binary;
using schema.binary.attributes;

namespace pm.schema.maps;

/// <summary>
///   Shamelessly stolen from:
///   https://github.com/magcius/noclip.website/blob/main/src/PaperMario64/map_shape.ts#L64
/// </summary>
[BinarySchema]
public sealed partial class Shape : IBinaryDeserializable {
  public const uint BASE_RAM_ADDRESS = 0x80210000;

  public static uint ConvertRamAddressToOffset(uint ramAddress)
    => ramAddress == 0 ? 0 : ramAddress - BASE_RAM_ADDRESS;

  public uint ModelTreeRootRamAddress { get; set; }
  public uint VertexTableRamAddress { get; set; }
  public uint ModelNameTableRamAddress { get; set; }
  public uint ColliderNameTableRamAddress { get; set; }
  public uint ZoneNameTableRamAddress { get; set; }

  [Skip]
  public uint ModelTreeRootOffset
    => ConvertRamAddressToOffset(this.ModelTreeRootRamAddress);

  [RAtPosition(nameof(ModelTreeRootOffset))]
  public ModelTreeNode ModelTreeRoot { get; } = new();

  [Skip]
  public uint VertexTableOffset
    => ConvertRamAddressToOffset(this.VertexTableRamAddress);

  [Skip]
  public uint ModelNameTableOffset
    => ConvertRamAddressToOffset(this.ModelNameTableRamAddress);

  [Skip]
  public uint ColliderNameTableOffset
    => ConvertRamAddressToOffset(this.ColliderNameTableRamAddress);

  [Skip]
  public uint ZoneNameTableOffset
    => ConvertRamAddressToOffset(this.ZoneNameTableRamAddress);
}