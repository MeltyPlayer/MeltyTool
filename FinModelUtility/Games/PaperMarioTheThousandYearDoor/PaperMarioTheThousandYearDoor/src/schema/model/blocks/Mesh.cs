using schema.binary;
using schema.binary.attributes;

namespace ttyd.schema.model.blocks;

[BinarySchema]
public sealed partial class Mesh : IBinaryDeserializable {
  [SequenceLengthSource(4)]
  public int[] Unks1 { get; set; }

  public int SamplerIndex { get; set; }

  public uint TevMode { get; set; }

  [SequenceLengthSource(8)]
  public int[] Unks2 { get; set; }

  public int PolygonBaseIndex { get; set; }
  public int PolygonCount { get; set; }

  public int VertexPositionBaseIndex { get; set; }
  public int VertexNormalBaseIndex { get; set; }
  public int VertexColorBaseIndex { get; set; }

  [SequenceLengthSource(8)]
  public int[] VertexTexCoordBaseIndices { get; set; }

  public override string ToString() => $"samplerIndex: {this.SamplerIndex}";
}