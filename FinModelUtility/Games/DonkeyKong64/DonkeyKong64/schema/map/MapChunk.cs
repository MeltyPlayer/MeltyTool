using schema.binary;

namespace dk64.schema.map;

/// <summary>
///   Shamelessly stolen from:
///   https://github.com/magcius/noclip.website/blob/main/src/DonkeyKong64/scenes.ts#L544
/// </summary>
[BinarySchema]
public sealed partial class MapChunk : IBinaryDeserializable {
  public int X { get; set; }
  public int Y { get; set; }
  public uint Unk0 { get; set; }

  public OffsetAndSize[] DlTable { get; } = new OffsetAndSize[4];

  public OffsetAndSize VertexOffsetAndSize { get; set; }
}

[BinarySchema]
public partial struct OffsetAndSize : IBinaryDeserializable {
  public int Offset { get; set; }
  public int Size { get; set; }
}