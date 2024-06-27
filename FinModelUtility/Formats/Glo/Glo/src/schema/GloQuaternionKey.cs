using schema.binary;

namespace glo.schema;

[BinarySchema]
public sealed partial class GloQuaternionKey : IBinaryConvertible {
  public uint Time { get; set; }

  public float X { get; set; }
  public float Y { get; set; }
  public float Z { get; set; }
  public float W { get; set; }
}