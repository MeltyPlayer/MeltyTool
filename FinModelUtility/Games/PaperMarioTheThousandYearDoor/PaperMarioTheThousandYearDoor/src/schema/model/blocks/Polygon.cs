using schema.binary;

namespace ttyd.schema.model.blocks {
  [BinarySchema]
  public partial class Polygon : IBinaryDeserializable {
    public int VertexIndex { get; set; }
    public uint VertexCount { get; set; }
  }
}