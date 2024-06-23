using schema.binary;
using schema.binary.attributes;

namespace ttyd.schema.model.blocks {
  [BinarySchema]
  public partial class Mesh : IBinaryDeserializable {
    [SequenceLengthSource(4)]
    public int[] Unks1 { get; set; }

    public int SamplerIndex { get; set; }

    [SequenceLengthSource(9)]
    public int[] Unks2 { get; set; }

    public int PolygonBaseIndex { get; set; }
    public int PolygonCount { get; set; }

    public int VertexPositionBaseIndex { get; set; }
    public int VertexNormalBaseIndex { get; set; }
    public int VertexColorBaseIndex { get; set; }
    public int VertexTexCoordBaseIndex { get; set; }

    [SequenceLengthSource(7)]
    public int[] Unks3 { get; set; }
  }
}