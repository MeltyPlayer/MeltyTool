using schema.binary;

namespace hm64.schema.mesh;

[BinarySchema]
public partial class Vertex : IBinaryDeserializable {
  public sbyte X { get; set; }
  public byte Y { get; set; }
  public sbyte Z { get; set; }
}