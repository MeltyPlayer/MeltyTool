using schema.binary;
using schema.binary.attributes;

namespace pm.schema.maps;

public enum InternalType : uint {
  LEAF = 0x02,
  GROUP = 0x05,
  ROOT = 0x07,
  SPECIAL = 0x0A,
}

/// <summary>
///   Shamelessly stolen from:
///   https://github.com/magcius/noclip.website/blob/main/src/PaperMario64/map_shape.ts#L114
/// </summary>
[BinarySchema]
public sealed partial class ModelTreeNode : IBinaryDeserializable {
  [Skip]
  public string Name { get; set; }

  public InternalType Type { get; set; }
  public uint DisplayDataRamAddress { get; set; }
  public uint NumProperties { get; set; }
  public uint PropertyTableRamAddress { get; set; }
  public uint GroupDataRamAddress { get; set; }

  [Skip]
  public uint DisplayDataOffset
    => Shape.ConvertRamAddressToOffset(this.DisplayDataRamAddress);

  [RAtPosition(nameof(DisplayDataOffset))]
  public uint DisplayListRamAddress { get; set; }

  [Skip]
  public uint DisplayListOffset
    => Shape.ConvertRamAddressToOffset(this.DisplayListRamAddress);

  [Skip]
  public uint PropertyTableOffset
    => Shape.ConvertRamAddressToOffset(this.PropertyTableRamAddress);

  [RAtPosition(nameof(PropertyTableOffset))]
  [RSequenceLengthSource(nameof(NumProperties))]
  public Property[] Properties { get; set; }

  [Skip]
  public uint GroupDataOffset
    => Shape.ConvertRamAddressToOffset(this.GroupDataRamAddress);

  [RAtPositionOrNull(nameof(GroupDataOffset))]
  public GroupData? GroupData { get; set; }
}