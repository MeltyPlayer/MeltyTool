using fin.math;

using schema.binary;
using schema.binary.attributes;

namespace mod.schema.mod {
  // THANKS:
  // https://github.com/KillzXGaming/010-Templates/blob/816cfc57e2ee998b953cf488e4fed25c54e7861a/Pikmin/MOD.bt#L312
  public class DisplayListFlagsByteView {
    public byte b1;
    public byte b2;
    public byte b3;
    public byte cullMode;
  }

  [BinarySchema]
  public partial class DisplayListFlags : IBinaryConvertible {
    [Skip]
    public DisplayListFlagsByteView byteView = new();

    public uint intView {
      get => BitLogic.ToUint32(this.byteView.b1,
                               this.byteView.b2,
                               this.byteView.b3,
                               this.byteView.cullMode);
      set => (this.byteView.b1,
              this.byteView.b2,
              this.byteView.b3,
              this.byteView.cullMode) = BitLogic.FromUint32(value);
    }
  }

  [BinarySchema]
  public partial class DisplayList : IBinaryConvertible {
    public DisplayListFlags flags = new();

    // THANKS: Yoshi2's mod2obj
    public uint cmdCount = 0;

    [Align(0x20)]
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