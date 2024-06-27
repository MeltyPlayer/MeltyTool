using fin.schema.vector;

using schema.binary;

namespace glo.schema;

[BinarySchema]
public sealed partial class GloXyzKey : IBinaryConvertible {
  public uint Time { get; set; }
  public Vector3f Xyz { get; } = new();
}