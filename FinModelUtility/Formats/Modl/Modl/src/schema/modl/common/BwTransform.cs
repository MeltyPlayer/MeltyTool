using fin.schema.vector;

using schema.binary;

namespace modl.schema.modl.common;

[BinarySchema]
public partial class BwTransform : IBinaryConvertible {
  public Vector3f Position { get; } = new();
  public Vector4f Rotation { get; } = new();
}