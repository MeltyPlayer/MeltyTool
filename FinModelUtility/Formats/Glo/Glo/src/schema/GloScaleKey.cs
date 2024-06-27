using fin.schema.vector;

using schema.binary;

namespace glo.schema;

[BinarySchema]
public sealed partial class GloScaleKey : IBinaryConvertible {
  public uint Time { get; set; }
  public Vector3f Scale { get; } = new();
}