using System.Numerics;

using schema.binary;

namespace glo.schema;

[BinarySchema]
public sealed partial class GloQuaternionKey : IBinaryConvertible {
  public uint Time { get; set; }
  public Quaternion Quaternion { get; set; }
}