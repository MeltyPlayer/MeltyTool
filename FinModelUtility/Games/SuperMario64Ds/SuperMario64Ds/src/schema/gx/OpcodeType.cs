namespace sm64ds.schema.gx;

/// <summary>
///   Shamelessly stolen from:
///   https://github.com/Arisotura/SM64DSe/blob/master/SM64DSFormats/BMD.cs
/// </summary>
public enum OpcodeType : byte {
  NOOP = 0x00,

  MATRIX_UNK_0x10 = 0x10,
  MATRIX_UNK_0x11 = 0x11,
  MATRIX_UNK_0x12 = 0x12,
  MATRIX_UNK_0x13 = 0x13,

  MATRIX_RESTORE = 0x14,

  MATRIX_UNK_0x15 = 0x15,
  MATRIX_UNK_0x16 = 0x16,
  MATRIX_UNK_0x17 = 0x17,
  MATRIX_UNK_0x18 = 0x18,
  MATRIX_UNK_0x19 = 0x19,
  MATRIX_UNK_0x1A = 0x1A,
  MATRIX_UNK_0x1B = 0x1B,
  MATRIX_UNK_0x1C = 0x1C,

  COLOR = 0x20,
  NORMAL = 0x21,
  TEXCOORD = 0x22,

  VERTEX_0x23 = 0x23,
  VERTEX_0x24 = 0x24,
  VERTEX_0x25 = 0x25,
  VERTEX_0x26 = 0x26,
  VERTEX_0x27 = 0x27,
  VERTEX_0x28 = 0x28,

  UNK_0x29 = 0x29,
  UNK_0x2A = 0x2A,
  UNK_0x2B = 0x2B,

  LIGHTING_UNK_0x30 = 0x30,
  LIGHTING_UNK_0x31 = 0x31,
  LIGHTING_UNK_0x32 = 0x32,
  LIGHTING_UNK_0x33 = 0x33,
  LIGHTING_UNK_0x34 = 0x34,

  BEGIN_VERTEX_LIST = 0x40,
  END_VERTEX_LIST = 0x41,
  VERTEX_0x40 = 0x40,

  UNK_0x50 = 0x50,
  UNK_0x60 = 0x60,
  UNK_0x70 = 0x70,
  UNK_0x71 = 0x71,
  UNK_0x72 = 0x72,
}

public static class OpcodeTypeExtensions {
  public static int GetLength(this OpcodeType opcode)
    => opcode switch {
        OpcodeType.NOOP => 0,
        
        OpcodeType.MATRIX_UNK_0x10 => 4,
        OpcodeType.MATRIX_UNK_0x11 => 0,
        OpcodeType.MATRIX_UNK_0x12 => 4,
        OpcodeType.MATRIX_UNK_0x13 => 4,

        OpcodeType.MATRIX_RESTORE => 4,

        OpcodeType.MATRIX_UNK_0x15 => 0,
        OpcodeType.MATRIX_UNK_0x16 => 64,
        OpcodeType.MATRIX_UNK_0x17 => 48,
        OpcodeType.MATRIX_UNK_0x18 => 64,
        OpcodeType.MATRIX_UNK_0x19 => 48,
        OpcodeType.MATRIX_UNK_0x1A => 36,
        OpcodeType.MATRIX_UNK_0x1B => 12,
        OpcodeType.MATRIX_UNK_0x1C => 12,

        OpcodeType.COLOR => 4,
        OpcodeType.NORMAL => 4,
        OpcodeType.TEXCOORD => 4,

        OpcodeType.VERTEX_0x23 => 8,
        OpcodeType.VERTEX_0x24 => 4,
        OpcodeType.VERTEX_0x25 => 4,
        OpcodeType.VERTEX_0x26 => 4,
        OpcodeType.VERTEX_0x27 => 4,
        OpcodeType.VERTEX_0x28 => 4,

        OpcodeType.UNK_0x29 => 4,
        OpcodeType.UNK_0x2A => 4,
        OpcodeType.UNK_0x2B => 4,

        OpcodeType.LIGHTING_UNK_0x30 => 4,
        OpcodeType.LIGHTING_UNK_0x31 => 4,
        OpcodeType.LIGHTING_UNK_0x32 => 4,
        OpcodeType.LIGHTING_UNK_0x33 => 4,
        OpcodeType.LIGHTING_UNK_0x34 => 128,

        OpcodeType.BEGIN_VERTEX_LIST => 4,
        OpcodeType.END_VERTEX_LIST => 0,

        OpcodeType.UNK_0x50 => 4,
        OpcodeType.UNK_0x60 => 4,
        OpcodeType.UNK_0x70 => 12,
        OpcodeType.UNK_0x71 => 8,
        OpcodeType.UNK_0x72 => 4,
        
        _ => throw new ArgumentOutOfRangeException(nameof(opcode), opcode, null)
    };
}