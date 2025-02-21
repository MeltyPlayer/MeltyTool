using schema.binary;

namespace sm64ds.schema.gx;

public enum PolygonType {
  TRIANGLES,
  QUADS,
  TRIANGLE_STRIP,
  QUAD_STRIP,
}

public class OpcodeReader {
  public static IOpcode[] ReadOpcodes(IBinaryReader br) {
    var opcodes = new LinkedList<IOpcode>();

    while (!br.Eof) {
      var opcode0 = (OpcodeType) br.ReadByte();
      var opcode1 = (OpcodeType) br.ReadByte();
      var opcode2 = (OpcodeType) br.ReadByte();
      var opcode3 = (OpcodeType) br.ReadByte();

      opcodes.AddLast(ReadOpcode_(br, opcode0));
      opcodes.AddLast(ReadOpcode_(br, opcode1));
      opcodes.AddLast(ReadOpcode_(br, opcode2));
      opcodes.AddLast(ReadOpcode_(br, opcode3));
    }

    return opcodes.ToArray();
  }

  private static IOpcode ReadOpcode_(IBinaryReader br, OpcodeType type) {
    switch (type) {
      case OpcodeType.NOOP: {
        return br.ReadNew<NoopOpcode>();
      }
      case OpcodeType.MATRIX_RESTORE: {
        return br.ReadNew<MatrixRestoreOpcode>();
      }
      case OpcodeType.COLOR: {
        return br.ReadNew<ColorOpcode>();
      }
      case OpcodeType.NORMAL: {
        return br.ReadNew<NormalOpcode>();
      }
      case OpcodeType.TEXCOORD: {
        return br.ReadNew<TexCoordOpcode>();
      }
      case OpcodeType.VERTEX_0x23: {
        return br.ReadNew<Vertex0x23Opcode>();
      }
      case OpcodeType.VERTEX_0x24: {
        return br.ReadNew<Vertex0x24Opcode>();
      }
      case OpcodeType.VERTEX_0x25: {
        return br.ReadNew<Vertex0x25Opcode>();
      }
      case OpcodeType.VERTEX_0x26: {
        return br.ReadNew<Vertex0x26Opcode>();
      }
      case OpcodeType.VERTEX_0x27: {
        return br.ReadNew<Vertex0x27Opcode>();
      }
      case OpcodeType.VERTEX_0x28: {
        return br.ReadNew<Vertex0x28Opcode>();
      }
      case OpcodeType.BEGIN_VERTEX_LIST: {
        return br.ReadNew<BeginVertexListOpcode>();
      }
      case OpcodeType.END_VERTEX_LIST: {
        return br.ReadNew<EndVertexListOpcode>();
      }
      case OpcodeType.MATRIX_UNK_0x10:
      case OpcodeType.MATRIX_UNK_0x11:
      case OpcodeType.MATRIX_UNK_0x12:
      case OpcodeType.MATRIX_UNK_0x13:
      case OpcodeType.MATRIX_UNK_0x15:
      case OpcodeType.MATRIX_UNK_0x16:
      case OpcodeType.MATRIX_UNK_0x17:
      case OpcodeType.MATRIX_UNK_0x18:
      case OpcodeType.MATRIX_UNK_0x19:
      case OpcodeType.MATRIX_UNK_0x1A:
      case OpcodeType.MATRIX_UNK_0x1B:
      case OpcodeType.MATRIX_UNK_0x1C:
      case OpcodeType.UNK_0x29:
      case OpcodeType.UNK_0x2A:
      case OpcodeType.UNK_0x2B:
      case OpcodeType.LIGHTING_UNK_0x30:
      case OpcodeType.LIGHTING_UNK_0x31:
      case OpcodeType.LIGHTING_UNK_0x32:
      case OpcodeType.LIGHTING_UNK_0x33:
      case OpcodeType.LIGHTING_UNK_0x34:
      case OpcodeType.UNK_0x50:
      case OpcodeType.UNK_0x60:
      case OpcodeType.UNK_0x70:
      case OpcodeType.UNK_0x71:
      case OpcodeType.UNK_0x72: {
        var opcode = new UnhandledOpcode(type);
        opcode.Read(br);
        return opcode;
      }
      default: throw new ArgumentOutOfRangeException(nameof(type), type, null);
    }
  }
}