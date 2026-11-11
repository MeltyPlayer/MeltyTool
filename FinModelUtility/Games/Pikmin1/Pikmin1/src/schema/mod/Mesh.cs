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
  public sealed partial class DisplayList : IBinaryConvertible {
    [SequenceLengthSource(3)]
    public byte[] Flags { get; set; }

    public ModCullMode CullMode { get; set; }

    // THANKS: Yoshi2's mod2obj
    private uint cmdCount;

    private uint dlistSize_;
    
    [AlignStart(0x20)]
    [RSequenceLengthSource(nameof(dlistSize_))]
    public byte[] dlistData { get; set; }
  }

  [BinarySchema]
  public sealed partial class MeshPacket : IBinaryConvertible {
    [SequenceLengthSource(SchemaIntegerType.UINT32)]
    public short[] indices;

    [SequenceLengthSource(SchemaIntegerType.UINT32)]
    public DisplayList[] displaylists;
  }

  [BinarySchema]
  public sealed partial class Mesh : IBinaryConvertible {
    public uint boneIndex = 0;
    public uint vtxDescriptor = 0;

    [SequenceLengthSource(SchemaIntegerType.UINT32)]
    public MeshPacket[] packets;
  }
}