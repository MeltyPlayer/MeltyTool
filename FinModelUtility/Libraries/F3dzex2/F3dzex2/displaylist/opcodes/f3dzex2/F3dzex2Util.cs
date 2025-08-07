using fin.math;
using fin.math.fixedPoint;

using schema.binary;


namespace f3dzex2.displaylist.opcodes.f3dzex2;

public static class F3dzex2Util {
  public static Tri1OpcodeCommand ParseTri1OpcodeCommand(IBinaryReader br)
    => new() {
        VertexIndexA = (byte) (br.ReadByte() >> 1),
        VertexIndexB = (byte) (br.ReadByte() >> 1),
        VertexIndexC = (byte) (br.ReadByte() >> 1),
    };

  public static Tri2OpcodeCommand ParseTri2OpcodeCommand(IBinaryReader br) {
    var a0 = (byte) (br.ReadByte() >> 1);
    var b0 = (byte) (br.ReadByte() >> 1);
    var c0 = (byte) (br.ReadByte() >> 1);

    var unk0 = br.ReadByte();
    var a1 = (byte) (br.ReadByte() >> 1);
    var b1 = (byte) (br.ReadByte() >> 1);
    var c1 = (byte) (br.ReadByte() >> 1);

    return new Tri2OpcodeCommand {
        VertexIndexA0 = a0,
        VertexIndexB0 = b0,
        VertexIndexC0 = c0,
        VertexIndexA1 = a1,
        VertexIndexB1 = b1,
        VertexIndexC1 = c1,
    };
  }

  public static SetOtherModeHOpcodeCommand ParseSetOtherModeHOpcodeCommand(
      IBinaryReader br) {
    br.Position--;

    var word0 = br.ReadUInt32();
    var word1 = br.ReadUInt32();

    var len = (ushort) word0.ExtractFromRight(0, 8);
    var sft = (ushort) word0.ExtractFromRight(8, 8);

    return new SetOtherModeHOpcodeCommand {
        Length = len,
        Shift = sft,
        Data = word1,
    };
  }

  public static SetOtherModeLOpcodeCommand ParseSetOtherModeLOpcodeCommand(
      IBinaryReader br) {
    br.Position--;

    var word0 = br.ReadUInt32();
    var word1 = br.ReadUInt32();

    var len = (ushort) word0.ExtractFromRight(0, 8);
    var sft = (ushort) word0.ExtractFromRight(8, 8);

    return new SetOtherModeLOpcodeCommand {
        Length = len,
        Shift = sft,
        Data = word1,
    };
  }
}