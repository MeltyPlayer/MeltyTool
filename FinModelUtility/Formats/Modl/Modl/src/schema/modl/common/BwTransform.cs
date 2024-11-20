using System.Numerics;

using schema.binary;

namespace modl.schema.modl.common;

[BinarySchema]
public partial class BwTransform : IBinaryConvertible {
  public Vector3 Position { get; private set; }
  public Vector4 Rotation { get; private set; }
}