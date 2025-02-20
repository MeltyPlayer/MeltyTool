using schema.binary;
using schema.binary.attributes;

public enum ModCullMode : byte {
  SHOW_FRONT_ONLY = 0,
  SHOW_BACK_ONLY = 1,
  SHOW_BOTH = 2,
  SHOW_NEITHER = 3,
}

namespace pikmin1.schema.mod {
  [BinarySchema]
  public partial class DisplayList : IBinaryConvertible {
    [SequenceLengthSource(3)]
    public byte[] Flags { get; set; }

    public ModCullMode CullMode { get; set; }

    // THANKS: Yoshi2's mod2obj
    public uint cmdCount = 0;

    [AlignStart(0x20)]
    [SequenceLengthSource(SchemaIntegerType.INT32)]
    public byte[] dlistData { get; set; }
  }

  [BinarySchema]
  public partial class MeshPacket : IBinaryConvertible {
    [SequenceLengthSource(SchemaIntegerType.UINT32)]
    public short[] indices;

    [SequenceLengthSource(SchemaIntegerType.UINT32)]
    public DisplayList[] displaylists;
  }

  [BinarySchema]
  public partial class Mesh : IBinaryConvertible {
    public uint boneIndex = 0;
    public uint vtxDescriptor = 0;

    [SequenceLengthSource(SchemaIntegerType.UINT32)]
    public MeshPacket[] packets;
  }
}