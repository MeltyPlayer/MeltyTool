using schema.binary;

namespace modl.schema.modl.common;

[BinarySchema]
public partial struct VertexPosition : IBinaryConvertible {
  public short X { get; private set; }
  public short Y { get; private set; }
  public short Z { get; private set; }
}