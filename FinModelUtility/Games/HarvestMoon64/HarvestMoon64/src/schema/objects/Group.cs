using schema.binary;
using schema.binary.attributes;

namespace hm64.schema.objects;

[BinarySchema]
public partial class Group : IBinaryDeserializable {
  public byte SpriteIndex { get; set; }

  [SequenceLengthSource(SchemaIntegerType.BYTE)]
  public Instance[] Instances { get; set; }
}