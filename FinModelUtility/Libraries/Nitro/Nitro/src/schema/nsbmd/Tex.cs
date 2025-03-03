using schema.binary;
using schema.binary.attributes;

namespace nitro.schema.nsbmd;

[BinarySchema]
public partial class Tex : IBinaryDeserializable {
  [SequenceLengthSource(4)]
  public byte[] Stamp { get; set; }
}