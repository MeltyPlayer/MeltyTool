namespace nitro.schema.nsbmd.mesh;

public enum Opcode : byte {
  // 0x_0
  NOP = 0x00,
  UNKNOWN_0x40 = 0x40,
  UNKNOWN_0x80 = 0x80,

  // 0x01
  END = 0x01,

  // 0x02
  UNKNOWN_0x02 = 0x02,

  // 0x03
  LOAD_MATRIX = 0x03,

  // 0x_4
  BIND_MATERIAL_1 = 0x04,
  BIND_MATERIAL_2 = 0x24,
  BIND_MATERIAL_3 = 0x44,

  // 0x05
  DRAW = 0x05,

  // 0x_6
  MULT_MATRIX_1 = 0x06,
  MULT_MATRIX_2 = 0x26, // w/ store position
  MULT_MATRIX_3 = 0x46, // w/ load position
  MULT_MATRIX_4 = 0x66, // w/ both store and load position

  // 0x_7
  UNKNOWN_0x07 = 0x07,
  UNKNOWN_0x47 = 0x47,

  // 0x08
  UNKNOWN_0x08 = 0x08,

  // 0x09
  STORE_MATRIX = 0x09,

  // 0x_b
  SCALE_UP = 0x0b,
  SCALE_DOWN = 0x2b,

  // 0x0c
  UNKNOWN_0x0c = 0x0c,

  // 0x0d
  UNKNOWN_0x0d = 0x0d,
}

public static class OpcodeExtensions {
  /// <summary>
  ///   Shamelessly copied from:
  ///   https://github.com/scurest/apicula/blob/07c4d8facdcb015d118bf26a29d37c8b41021bfd/src/nitro/render_cmds.rs#L164
  /// </summary>
  public static int GetLength(this Opcode opcode)
    => opcode switch {
        // 0x00
        Opcode.NOP or Opcode.UNKNOWN_0x40 or Opcode.UNKNOWN_0x80 => 0,

        // 0x01
        Opcode.END => 0,

        // 0x02
        Opcode.UNKNOWN_0x02 => 2,

        // 0x03
        Opcode.LOAD_MATRIX => 1,

        // 0x_4
        Opcode.BIND_MATERIAL_1
            or Opcode.BIND_MATERIAL_2
            or Opcode.BIND_MATERIAL_3 => 1,

        // 0x05
        Opcode.DRAW => 1,

        // 0x_6
        Opcode.MULT_MATRIX_1 => 3,
        Opcode.MULT_MATRIX_2 => 4,
        Opcode.MULT_MATRIX_3 => 4,
        Opcode.MULT_MATRIX_4 => 5,

        // 0x_7
        Opcode.UNKNOWN_0x07 => 1,
        Opcode.UNKNOWN_0x47 => 2,

        // 0x08
        Opcode.UNKNOWN_0x08 => 1,

        // 0x09
        // (not here because it is variable-length)

        // 0x_b
        Opcode.SCALE_UP
            or Opcode.SCALE_DOWN => 0,

        // 0x0c
        Opcode.UNKNOWN_0x0c => 2,

        // 0x0d
        Opcode.UNKNOWN_0x0d => 2,
    };
}