using schema.binary;
using schema.binary.attributes;

namespace mp64.schema.form;

/// <summary>
///   Shamelessly stolen from:
///   https://github.com/PartyPlanner64/PartyPlanner64/blob/980d043217823d0e5901ee93d3a50e36f728db10/packages/lib/models/FORM.ts#L396
/// </summary>
[BinarySchema]
public sealed partial class Vtx1Data : IFormChunkData {
  [SequenceLengthSource(SchemaIntegerType.UINT16)]
  public Vertex[] Vertices { get; set; }
}

[BinarySchema]
public sealed partial class Vertex : IBinaryConvertible {
  public short X { get; set; }
  public short Y { get; set; }
  public short Z { get; set; }

  public sbyte NormalX { get; set; }
  public sbyte NormalY { get; set; }
  public sbyte NormalZ { get; set; }
}