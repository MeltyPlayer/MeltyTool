using schema.binary;

namespace gx.vertex;

public class GxPrimitive {
  // TODO: Parse these as standalone opcodes first
  public static IList<GxPrimitive> ReadOpcodes(
      IBinaryReader br,
      ref GxVertexDescriptor vertexDescriptor) {
    var primitives = new List<GxPrimitive>();

    while (!br.Eof) {
      var opcode = br.ReadByte();
      var opcodeEnum = (GxOpcode) opcode;

      switch (opcodeEnum) {
        case GxOpcode.NOP: break;
        case GxOpcode.LOAD_CP_REG: {
          var command = br.ReadByte();
          var value = br.ReadUInt32();

          if (command == 0x50) {
            vertexDescriptor =
                new GxVertexDescriptor(
                    (vertexDescriptor.Value & ~((uint) 0x1FFFF)) |
                    value);
          } else if (command == 0x60) {
            vertexDescriptor =
                new GxVertexDescriptor(
                    (vertexDescriptor.Value & 0x1FFFF) |
                    (value << 17));
          } else {
            throw new NotImplementedException();
          }

          break;
        }
        case GxOpcode.LOAD_XF_REG: {
          var lengthMinusOne = br.ReadUInt16();
          var length = lengthMinusOne + 1;

          // http://hitmen.c02.at/files/yagcd/yagcd/chap5.html#sec5.11.4
          var firstXfRegisterAddress = br.ReadUInt16();

          var values = br.ReadUInt32s(length);
          // TODO: Implement
          break;
        }
        case GxOpcode.DRAW_POINTS:
        case GxOpcode.DRAW_LINES:
        case GxOpcode.DRAW_LINE_STRIP:
        case GxOpcode.DRAW_TRIANGLES:
        case GxOpcode.DRAW_TRIANGLE_FAN:
        case GxOpcode.DRAW_TRIANGLE_STRIP:
        case GxOpcode.DRAW_QUADS:
        case GxOpcode.DRAW_QUADS_2: {
          primitives.Add(ReadPrimitive(br, vertexDescriptor));
          break;
        }
      }
    }

    return primitives;
  }

  public static GxPrimitive ReadPrimitive(
      IBinaryReader br,
      GxVertexDescriptor vertexDescriptor) {
    var vertexCount = br.ReadUInt16();

    for (var i = 0; i < vertexCount; i++) {
      foreach (var (attribute, attributeType) in vertexDescriptor) {

      }
    }

    return null!;
  }
}