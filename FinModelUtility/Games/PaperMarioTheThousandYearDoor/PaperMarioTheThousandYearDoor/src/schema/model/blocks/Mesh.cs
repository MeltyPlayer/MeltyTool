using schema.binary;
using schema.binary.attributes;

namespace ttyd.schema.model.blocks {
  [BinarySchema]
  public partial class Mesh : IBinaryDeserializable {
    [SequenceLengthSource(4)]
    public int Unks1 { get; set; }

    public int TexMapIndex { get; set; }

    [SequenceLengthSource(9)]
    public int Unks2 { get; set; }

    public int PolygonIndex { get; set; }
    public int PolygonCount { get; set; }

    public int PolyVertexIndex { get; set; }
    public int PolyNormalIndex { get; set; }
    public int PolyColorIndex { get; set; }
    public int PolyTexCoordIndex { get; set; }

    [SequenceLengthSource(7)]
    public int Unks3 { get; set; }
  }
}