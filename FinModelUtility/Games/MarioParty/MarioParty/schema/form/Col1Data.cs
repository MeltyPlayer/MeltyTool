using fin.schema.color;

using schema.binary;
using schema.binary.attributes;

namespace mp64.schema.form;

/// <summary>
///   Shamelessly stolen from:
///   https://github.com/PartyPlanner64/PartyPlanner64/blob/980d043217823d0e5901ee93d3a50e36f728db10/packages/lib/models/FORM.ts#L338
/// </summary>
[BinarySchema]
public sealed partial class Col1Data : IFormChunkData {
  [SequenceLengthSource(SchemaIntegerType.UINT16)]
  public Rgba32[] Colors { get; set; }
}