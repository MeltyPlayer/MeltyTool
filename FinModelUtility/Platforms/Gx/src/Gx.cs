namespace gx;

/// <summary>
///   Shamelessly copied from https://github.com/magcius/noclip.website/blob/1fa39ab5d9d014095c77a8509bf6ba87d3296200/src/gx/gx_enum.ts
/// </summary>
public enum GxOpcode : byte {
  NOP = 0x0,

  DRAW_QUADS = 0x80,

  // Early code for GX_DRAW_QUADS? Seen in Luigi's Mansion.
  DRAW_QUADS_2 = 0x88,
  DRAW_TRIANGLES = 0x90,
  DRAW_TRIANGLE_STRIP = 0x98,
  DRAW_TRIANGLE_FAN = 0xA0,
  DRAW_LINES = 0xA8,
  DRAW_LINE_STRIP = 0xB0,
  DRAW_POINTS = 0xB8,

  LOAD_INDX_A = 0x20,
  LOAD_INDX_B = 0x28,
  LOAD_INDX_C = 0x30,
  LOAD_INDX_D = 0x38,

  LOAD_BP_REG = 0x61,
  LOAD_CP_REG = 0x08,
  LOAD_XF_REG = 0x10,
}

public enum GxComponentCount {
  // Position
  POS_XY = 0,
  POS_XYZ = 1,

  // Normal
  NRM_XYZ = 0,
  NRM_NBT = 1,
  NRM_NBT3 = 2,

  // Color
  CLR_RGB = 0,
  CLR_RGBA = 1,

  // TexCoord
  TEX_S = 0,
  TEX_ST = 1,
}

public enum GxComponentType {
  U8 = 0,
  S8 = 1,
  U16 = 2,
  S16 = 3,
  F32 = 4,

  RGB565 = 0,
  RGB8 = 1,
  RGBX8 = 2,
  RGBA4 = 3,
  RGBA6 = 4,
  RGBA8 = 5,
}

public enum GxTexMap : byte {
  GX_TEXMAP0,
  GX_TEXMAP1,
  GX_TEXMAP2,
  GX_TEXMAP3,
  GX_TEXMAP4,
  GX_TEXMAP5,
  GX_TEXMAP6,
  GX_TEXMAP7,
  GX_TEXMAP_NULL = 0xff,
}

public enum GxTexCoord : byte {
  GX_TEXCOORD0,
  GX_TEXCOORD1,
  GX_TEXCOORD2,
  GX_TEXCOORD3,
  GX_TEXCOORD4,
  GX_TEXCOORD5,
  GX_TEXCOORD6,
  GX_TEXCOORD7,
  GX_TEXCOORD_NULL = 0xff,
}

public enum GxKonstColorSel : byte {
  KCSel_1 = 0x00,    // Constant 1.0
  KCSel_7_8 = 0x01,  // Constant 7/8
  KCSel_3_4 = 0x02,  // Constant 3/4
  KCSel_5_8 = 0x03,  // Constant 5/8
  KCSel_1_2 = 0x04,  // Constant 1/2
  KCSel_3_8 = 0x05,  // Constant 3/8
  KCSel_1_4 = 0x06,  // Constant 1/4
  KCSel_1_8 = 0x07,  // Constant 1/8
  KCSel_K0 = 0x0C,   // K0[RGB] Register
  KCSel_K1 = 0x0D,   // K1[RGB] Register
  KCSel_K2 = 0x0E,   // K2[RGB] Register
  KCSel_K3 = 0x0F,   // K3[RGB] Register
  KCSel_K0_R = 0x10, // K0[RRR] Register
  KCSel_K1_R = 0x11, // K1[RRR] Register
  KCSel_K2_R = 0x12, // K2[RRR] Register
  KCSel_K3_R = 0x13, // K3[RRR] Register
  KCSel_K0_G = 0x14, // K0[GGG] Register
  KCSel_K1_G = 0x15, // K1[GGG] Register
  KCSel_K2_G = 0x16, // K2[GGG] Register
  KCSel_K3_G = 0x17, // K3[GGG] Register
  KCSel_K0_B = 0x18, // K0[BBB] Register
  KCSel_K1_B = 0x19, // K1[BBB] Register
  KCSel_K2_B = 0x1A, // K2[BBB] Register
  KCSel_K3_B = 0x1B, // K3[BBB] Register
  KCSel_K0_A = 0x1C, // K0[AAA] Register
  KCSel_K1_A = 0x1D, // K1[AAA] Register
  KCSel_K2_A = 0x1E, // K2[AAA] Register
  KCSel_K3_A = 0x1F  // K3[AAA] Register
}

public enum GxKonstAlphaSel : byte {
  KASel_1 = 0x00,    // Constant 1.0
  KASel_7_8 = 0x01,  // Constant 7/8
  KASel_3_4 = 0x02,  // Constant 3/4
  KASel_5_8 = 0x03,  // Constant 5/8
  KASel_1_2 = 0x04,  // Constant 1/2
  KASel_3_8 = 0x05,  // Constant 3/8
  KASel_1_4 = 0x06,  // Constant 1/4
  KASel_1_8 = 0x07,  // Constant 1/8
  KASel_K0_R = 0x10, // K0[R] Register
  KASel_K1_R = 0x11, // K1[R] Register
  KASel_K2_R = 0x12, // K2[R] Register
  KASel_K3_R = 0x13, // K3[R] Register
  KASel_K0_G = 0x14, // K0[G] Register
  KASel_K1_G = 0x15, // K1[G] Register
  KASel_K2_G = 0x16, // K2[G] Register
  KASel_K3_G = 0x17, // K3[G] Register
  KASel_K0_B = 0x18, // K0[B] Register
  KASel_K1_B = 0x19, // K1[B] Register
  KASel_K2_B = 0x1A, // K2[B] Register
  KASel_K3_B = 0x1B, // K3[B] Register
  KASel_K0_A = 0x1C, // K0[A] Register
  KASel_K1_A = 0x1D, // K1[A] Register
  KASel_K2_A = 0x1E, // K2[A] Register
  KASel_K3_A = 0x1F  // K3[A] Register
}

public enum GxTextureFormat : byte {
  I4 = 0,
  I8 = 1,
  A4_I4 = 2,
  A8_I8 = 3,
  R5_G6_B5 = 4,
  A3_RGB5 = 5,
  ARGB8 = 6,
  INDEX4 = 8,
  INDEX8 = 9,
  INDEX14_X2 = 10, // 0x0000000A
  S3TC1 = 14,      // 0x0000000E
}

public enum GxPaletteFormat : byte {
  PAL_A8_I8 = 0,
  PAL_R5_G6_B5 = 1,
  PAL_A3_RGB5 = 2,
}