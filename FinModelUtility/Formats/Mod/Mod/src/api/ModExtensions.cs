using fin.model;

using gx;

using mod.schema.mod;

namespace mod.api;

public static class ModExtensions {
  public static WrapMode ConvertGcnToFin(this TilingMode tilingMode)
    => tilingMode switch {
        TilingMode.CLAMP         => WrapMode.CLAMP,
        TilingMode.MIRROR_REPEAT => WrapMode.MIRROR_REPEAT,
        (TilingMode) 3           => WrapMode.MIRROR_CLAMP,
        _                        => WrapMode.REPEAT,
    };

  public static GxWrapMode ConvertGcnToGx(this TilingMode tilingMode)
    => tilingMode switch {
        TilingMode.CLAMP         => GxWrapMode.GX_CLAMP,
        TilingMode.MIRROR_REPEAT => GxWrapMode.GX_MIRROR,
        (TilingMode) 3           => GxWrapMode.GX_REPEAT,
        _                        => GxWrapMode.GX_REPEAT,
    };
}