using schema.binary;

namespace berserkDc.schema;

[BinarySchema]
public sealed partial class NjcmBone : IBinaryDeserializable {
  public uint Flags { get; set; }
  public uint MeshOffset { get; set; }

}
