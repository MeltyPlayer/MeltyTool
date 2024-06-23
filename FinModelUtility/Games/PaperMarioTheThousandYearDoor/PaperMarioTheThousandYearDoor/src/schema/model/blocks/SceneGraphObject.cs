using schema.binary;
using schema.binary.attributes;

namespace ttyd.schema.model.blocks {
  [BinarySchema]
  public partial class SceneGraphObject : IBinaryDeserializable {
    [StringLengthSource(64)]
    public string Name { get; set; }

    public int VertexPositionBaseIndex { get; set; }
    public int VertexPositionCount { get; set; }

    public int VertexNormalBaseIndex { get; set; }
    public int VertexNormalCount { get; set; }

    public int VertexColorBaseIndex { get; set; }
    public int VertexColorCount { get; set; }

    public int VertexTexCoordBaseIndex { get; set; }
    public int VertexTexCoordCount { get; set; }

    [SequenceLengthSource(14)]
    public int[] Unk1 { get; set; }

    public int MeshBaseIndex { get; set; }
    public int MeshCount { get; set; }

    public int Blending { get; set; }
    public int Culling { get; set; }
  }
}