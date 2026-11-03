using fin.schema;

using schema.binary;
using schema.binary.attributes;

namespace mp64.schema.form;

/// <summary>
///   Shamelessly stolen from:
///   https://github.com/PartyPlanner64/PartyPlanner64/blob/980d043217823d0e5901ee93d3a50e36f728db10/packages/lib/models/FORM.ts#L350
/// </summary>
[BinarySchema]
public sealed partial class Mat1Data : IFormChunkData {
  [SequenceLengthSource(SchemaIntegerType.UINT16)]
  public Material[] Materials { get; set; }
}

[BinarySchema]
public sealed partial class Material : IFormChunkData {
  [Unknown]
  public ushort Unk0 { get; set; }
  public ushort ColorIndex { get; set; }
  [Unknown]
  public ushort Unk1 { get; set; }
  [Unknown]
  public float Unk2 { get; set; }
  [Unknown]
  public ushort Unk3 { get; set; }
}