using fin.schema;

using schema.binary;
using schema.binary.attributes;

namespace mp64.schema.form;

/// <summary>
///   Shamelessly stolen from:
///   https://github.com/PartyPlanner64/PartyPlanner64/blob/980d043217823d0e5901ee93d3a50e36f728db10/packages/lib/models/FORM.ts#L497
/// </summary>
[BinarySchema]
public sealed partial class Skl1Data : IFormChunkData {
  public ushort GlobalIndex { get; set; }

  [SequenceLengthSource(SchemaIntegerType.BYTE)]
  public Bone[] Bones { get; set; }
}

[BinarySchema]
public sealed partial class Bone : IBinaryConvertible {
  [Unknown]
  public byte Unk0 { get; set; }

  public ushort ObjGlobalIndex { get; set; }
  public Transform Transform { get; } = new();
  [Unknown]
  public float Unk1 { get; set; }
  [Unknown]
  public float Unk2 { get; set; }
  [Unknown]
  public float Unk3 { get; set; }
  public ushort NextSiblingRelativeIndex { get; set; }
  public byte IsParentNode { get; set; }

  [Unknown]
  public byte Unk4 { get; set; }
}