namespace glo.schema {
  [Flags]
  public enum GloObjectFlags : ushort {
    GOURAUD_SHADED = 1 << 0,
    TRANSLUCENT = 1 << 1,
    TRANSPARENT = 1 << 2,
    PRELIT = 1 << 3,
    MIP_MAP = 1 << 4,
    TEXTURE_BLEND = 1 << 6,
    ALPHA_TEXTURE = 1 << 7,
    PHONG_SHADED = 1 << 8,
    FACE_COLOR = 1 << 10,
    OBJECT_COLOR = 1 << 11,
    VERTEX_ALPHA = 1 << 15,
  }
}