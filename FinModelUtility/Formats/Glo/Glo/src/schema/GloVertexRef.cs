using schema.binary;

namespace glo.schema;

[BinarySchema]
public sealed partial class GloVertexRef : IBinaryConvertible {
  public ushort Index { get; set; }
  public float U { get; set; }
  public float V { get; set; }
}