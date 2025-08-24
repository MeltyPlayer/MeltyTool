using fin.schema.color;

using schema.binary;
using schema.binary.attributes;


namespace marioartist.schema;

[BinarySchema]
[Endianness(Endianness.BigEndian)]
public partial class Thumbnail : IBinaryDeserializable {
  public Argb1555Image Image { get; } = new(24, 24);
}