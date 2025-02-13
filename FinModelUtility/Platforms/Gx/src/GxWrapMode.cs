using fin.model;

namespace gx;

public enum GxWrapMode : byte {
  GX_CLAMP,
  GX_REPEAT,
  GX_MIRROR,
}

public static class GxWrapModeExtensions {
  public static WrapMode ToFinWrapMode(this GxWrapMode gxWrapMode)
    => gxWrapMode switch {
        GxWrapMode.GX_CLAMP  => WrapMode.CLAMP,
        GxWrapMode.GX_REPEAT => WrapMode.REPEAT,
        GxWrapMode.GX_MIRROR => WrapMode.MIRROR_REPEAT,
        _                    => throw new ArgumentOutOfRangeException(nameof(gxWrapMode), gxWrapMode, null)
    };
}