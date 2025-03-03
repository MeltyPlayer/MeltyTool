using schema.binary;

namespace nitro.schema.nsbmd.mesh;

public interface IOpcodeParser {
  IList<IOpcode> Parse(IBinaryReader br);
}

/// <summary>
///   Shamelessly stolen from:
///   https://github.com/scurest/apicula/blob/07c4d8facdcb015d118bf26a29d37c8b41021bfd/src/nitro/render_cmds.rs#L40
/// </summary>
public class OpcodeParser : IOpcodeParser {
  public IList<IOpcode> Parse(IBinaryReader br) {
    var opcodes = new List<IOpcode>();
    while (true) {
      GetNextOpcodeAndParams_(br, out var opcodeType, out var paramsList);

      switch (opcodeType) {
        case OpcodeType.END: {
          return opcodes;
        }
        case OpcodeType.LOAD_MATRIX: {
          opcodes.Append(new LoadMatrixOpcode(paramsList[0]));
          break;
        }
        case OpcodeType.BIND_MATERIAL_1
             or OpcodeType.BIND_MATERIAL_2
             or OpcodeType.BIND_MATERIAL_3: {
          opcodes.Append(new BindMaterialOpcode(paramsList[0]));
          break;
        }
        case OpcodeType.DRAW: {
          opcodes.Append(new DrawOpcode(paramsList[0]));
          break;
        }
        case OpcodeType.MULT_MATRIX_1
             or OpcodeType.MULT_MATRIX_2
             or OpcodeType.MULT_MATRIX_3
             or OpcodeType.MULT_MATRIX_4: {
          var objectIndex = paramsList[0];
          var parentId = paramsList[1];
          var unk = paramsList[2];

          byte? storePos = null;
          byte? loadPos = null;
          switch (opcodeType) {
            case OpcodeType.MULT_MATRIX_2: {
              storePos = paramsList[3];
              break;
            }
            case OpcodeType.MULT_MATRIX_3: {
              loadPos = paramsList[3];
              break;
            }
            case OpcodeType.MULT_MATRIX_4: {
              storePos = paramsList[3];
              loadPos = paramsList[4];
              break;
            }
          }

          if (loadPos != null) {
            opcodes.Add(new LoadMatrixOpcode(loadPos.Value));
          }

          opcodes.Add(new MultObjectOpcode(objectIndex));

          if (storePos != null) {
            opcodes.Add(new StoreMatrixOpcode(storePos.Value));
          }

          break;
        }
        case OpcodeType.STORE_MATRIX: {
          var storePos = paramsList[0];
          var numTerms = paramsList[1];

          var terms = new List<ISkinTerm>();
          for (var i = 0; i < numTerms; ++i) {
            var stackPos = paramsList[2 + 3 * i + 0];
            var inverseBindIndex = paramsList[2 + 3 * i + 1];
            var weight = paramsList[2 + 3 * i + 2] / 256f;

            terms.Add(new SkinTerm(stackPos, inverseBindIndex, weight));
          }

          opcodes.Add(new SkinOpcode(terms));
          opcodes.Add(new StoreMatrixOpcode(storePos));
          break;
        }
        case OpcodeType.SCALE_UP: {
          opcodes.Append(new ScaleUpOpcode());
          break;
        }
        case OpcodeType.SCALE_DOWN: {
          opcodes.Append(new ScaleDownOpcode());
          break;
        }
      }
    }
  }

  private static void GetNextOpcodeAndParams_(IBinaryReader br,
                                              out OpcodeType opcodeType,
                                              out byte[] paramsList) {
    opcodeType = (OpcodeType) br.ReadByte();

    var paramsLength = opcodeType switch {
        // 0x00
        OpcodeType.NOP
            or OpcodeType.UNKNOWN_0x40
            or OpcodeType.UNKNOWN_0x80 => 0,

        // 0x01
        OpcodeType.END => 0,

        // 0x02
        OpcodeType.UNKNOWN_0x02 => 2,

        // 0x03
        OpcodeType.LOAD_MATRIX => 1,

        // 0x_4
        OpcodeType.BIND_MATERIAL_1
            or OpcodeType.BIND_MATERIAL_2
            or OpcodeType.BIND_MATERIAL_3 => 1,

        // 0x05
        OpcodeType.DRAW => 1,

        // 0x_6
        OpcodeType.MULT_MATRIX_1 => 3,
        OpcodeType.MULT_MATRIX_2 => 4,
        OpcodeType.MULT_MATRIX_3 => 4,
        OpcodeType.MULT_MATRIX_4 => 5,

        // 0x_7
        OpcodeType.UNKNOWN_0x07 => 1,
        OpcodeType.UNKNOWN_0x47 => 2,

        // 0x08
        OpcodeType.UNKNOWN_0x08 => 1,

        // 0x09
        OpcodeType.STORE_MATRIX => 1 + 1 + 3 * br.ReadByte(),

        // 0x_b
        OpcodeType.SCALE_UP or OpcodeType.SCALE_DOWN => 0,

        // 0x0c
        OpcodeType.UNKNOWN_0x0c => 2,

        // 0x0d
        OpcodeType.UNKNOWN_0x0d => 2,

        _                       => throw new ArgumentOutOfRangeException(nameof(opcodeType), opcodeType, null)
    };
    paramsList = new byte[paramsLength];
  }
}