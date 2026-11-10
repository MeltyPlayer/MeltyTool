using fin.schema;
using fin.schema.vector;

using schema.binary;
using schema.binary.attributes;

namespace mp64.schema.form;

/// <summary>
///   Shamelessly stolen from:
///   https://github.com/PartyPlanner64/PartyPlanner64/blob/980d043217823d0e5901ee93d3a50e36f728db10/packages/lib/models/FORM.ts#L396
/// </summary>
[BinarySchema]
public sealed partial class Vtx1Data : IFormChunkData {
  private ushort vertexCount_;

  [Unknown]
  public ushort Unk0 { get; set; }

  public float Scale { get; set; }

  [RSequenceLengthSource(nameof(vertexCount_))]
  public Vertex[] Vertices { get; set; }
}

[BinarySchema]
public sealed partial class Vertex : IBinaryConvertible {
  public Vector3s Position { get; } = new();
  public Vector3b Normal { get; } = new();
}