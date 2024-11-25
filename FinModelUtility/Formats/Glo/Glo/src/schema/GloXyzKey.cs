using System.Numerics;

using schema.binary;

namespace glo.schema;

[BinarySchema]
public sealed partial class GloXyzKey : IBinaryConvertible {
  public uint Time { get; set; }
  public Vector3 Xyz { get; private set; }
}