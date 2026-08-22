using schema.binary;
using schema.binary.attributes;

namespace hm64.schema.objects;

[BinarySchema]
public partial class Instance : IBinaryDeserializable {
  public byte Flags { get; set; }

  [Endianness(Endianness.LittleEndian)]
  public short X { get; set; }

  [Endianness(Endianness.LittleEndian)]
  public short Y { get; set; }

  [Endianness(Endianness.LittleEndian)]
  public short Z { get; set; }
}