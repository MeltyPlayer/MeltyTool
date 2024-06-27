namespace grezzo.schema.ctxb;

public enum GlTextureFormat : uint {
  Shadow = 0x00006040,
  ETC1 = 0x0000675A,   // or 0x1401675A,
  ETC1a4 = 0x0000675B, // or 0x1401675B,
  RGB8 = 0x14016754,
  RGBA8 = 0x14016752,
  RGBA4444 = 0x80336752,
  RGBA5551 = 0x80346752,
  RGB565 = 0x83636754,
  LA8 = 0x14016758,
  Gas = 0x00006050,
  HiLo8 = 0x14016759,
  A8 = 0x14016756,
  L8 = 0x14016757,
  LA4 = 0x67606758,
  L4 = 0x67616757,
  A4 = 0x67616756,
}

public static class GlTextureFormatExtensions {
  public static bool IsRgb(this GlTextureFormat format)
    => format is GlTextureFormat.RGB8 or GlTextureFormat.RGB565;

  public static bool IsRgba(this GlTextureFormat format)
    => format is GlTextureFormat.RGBA8
                 or GlTextureFormat.RGBA4444
                 or GlTextureFormat.RGBA5551;

  public static bool IsIntensityAlpha(this GlTextureFormat format)
    => format is GlTextureFormat.L4
                 or GlTextureFormat.L8
                 or GlTextureFormat.Gas
                 or GlTextureFormat.Shadow
                 or GlTextureFormat.LA4
                 or GlTextureFormat.LA8;

  public static bool IsAlpha(this GlTextureFormat format)
    => format is GlTextureFormat.A4 or GlTextureFormat.A8;

  public static bool IsEtc1(this GlTextureFormat format, out bool hasAlpha) {
      hasAlpha = format == GlTextureFormat.ETC1a4;
      return format is GlTextureFormat.ETC1 or GlTextureFormat.ETC1a4;
    }
}