using schema.binary;
using schema.binary.attributes;

namespace ttyd.schema.model.blocks {
  [BinarySchema]
  public partial class SceneGraphObject : IBinaryDeserializable {
    [StringLengthSource(64)]
    public string Name { get; set; }

    public int VertexIndex { get; set; }
    public int VertexCount { get; set; }

    public int NormalIndex { get; set; }
    public int NormalCount { get; set; }

    public int ColorIndex { get; set; }
    public int ColorCount { get; set; }

    public int TexCoordIndex { get; set; }
    public int TexCoordCount { get; set; }

    [SequenceLengthSource(14)]
    public int[] Unk1 { get; set; }

    public int MeshIndex { get; set; }
    public int MeshCount { get; set; }

    public int Blending { get; set; }
    public int Culling { get; set; }
  }
}