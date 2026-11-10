using schema.binary;
using schema.binary.attributes;

namespace hm64.schema;

/// <summary>
///   Shamelessly stolen from:
///   https://github.com/harvestwhisperer/hm64-decomp/blob/master/tools/modding/map/blender_import.py#L484
/// </summary>
[BinarySchema]
public partial class Grid : IBinaryDeserializable {
  private uint pointer_;

  public byte TileSizeX { get; set; }
  public byte TileSizeZ { get; set; }

  public byte MapWidth { get; set; }
  public byte MapHeight { get; set; }

  private uint unk0_;

  [Skip]
  private int TileCount_ => this.MapWidth * this.MapHeight;

  [RSequenceLengthSource(nameof(TileCount_))]
  public TileIndex[] TileIndices { get; set; }
}

[BinarySchema]
[Endianness(Endianness.LittleEndian)]
public partial struct TileIndex : IBinaryDeserializable {
  public ushort Value { get; set; }
}