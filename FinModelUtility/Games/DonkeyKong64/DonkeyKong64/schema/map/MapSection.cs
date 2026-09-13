using schema.binary;

namespace dk64.schema.map;

/// <summary>
///   Shamelessly stolen from:
///   https://github.com/magcius/noclip.website/blob/main/src/DonkeyKong64/scenes.ts#L573
/// </summary>
[BinarySchema]
public sealed partial class MapSection : IBinaryDeserializable {
  public ushort Unk0 { get; set; }
  public ushort MeshId { get; set; }
  public uint Unk1 { get; set; }
  public ushort[] VertexOffsets { get; } = new ushort[8];
  public uint Unk2 { get; set; }
}