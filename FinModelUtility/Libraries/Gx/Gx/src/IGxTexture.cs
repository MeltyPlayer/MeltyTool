using fin.image;
using fin.model;

namespace gx;

public enum GX_MAG_TEXTURE_FILTER : byte {
  GX_NEAR,
  GX_LINEAR,
}

public enum GX_MIN_TEXTURE_FILTER : byte {
  GX_NEAR,
  GX_LINEAR,
  GX_NEAR_MIP_NEAR,
  GX_LIN_MIP_NEAR,
  GX_NEAR_MIP_LIN,
  GX_LIN_MIP_LIN,
  GX_NEAR2,
  GX_NEAR3,
}

public static class GxTextureFilterExtensions {
  public static TextureMagFilter ToFinMagFilter(
      this GX_MAG_TEXTURE_FILTER gxMagFilter)
    => gxMagFilter switch {
        GX_MAG_TEXTURE_FILTER.GX_NEAR   => TextureMagFilter.NEAR,
        GX_MAG_TEXTURE_FILTER.GX_LINEAR => TextureMagFilter.LINEAR,
        _                               => throw new ArgumentOutOfRangeException(nameof(gxMagFilter), gxMagFilter, null)
    };

  public static TextureMinFilter ToFinMinFilter(
      this GX_MIN_TEXTURE_FILTER gxMinFilter)
    => gxMinFilter switch {
        GX_MIN_TEXTURE_FILTER.GX_NEAR   => TextureMinFilter.NEAR,
        GX_MIN_TEXTURE_FILTER.GX_LINEAR => TextureMinFilter.LINEAR,
        GX_MIN_TEXTURE_FILTER.GX_NEAR_MIP_NEAR => TextureMinFilter
            .NEAR_MIPMAP_NEAR,
        GX_MIN_TEXTURE_FILTER.GX_LIN_MIP_NEAR => TextureMinFilter
            .LINEAR_MIPMAP_NEAR,
        GX_MIN_TEXTURE_FILTER.GX_NEAR_MIP_LIN => TextureMinFilter
            .NEAR_MIPMAP_LINEAR,
        GX_MIN_TEXTURE_FILTER.GX_LIN_MIP_LIN => TextureMinFilter
            .LINEAR_MIPMAP_LINEAR,
        GX_MIN_TEXTURE_FILTER.GX_NEAR2 => TextureMinFilter.NEAR,
        GX_MIN_TEXTURE_FILTER.GX_NEAR3 => TextureMinFilter.NEAR,
    };
};

public interface IGxTexture {
  string Name { get; }
  IReadOnlyImage[] MipmapImages { get; }
  GxWrapMode WrapModeS { get; }
  GxWrapMode WrapModeT { get; }
  GX_MAG_TEXTURE_FILTER MagTextureFilter { get; }
  GX_MIN_TEXTURE_FILTER MinTextureFilter { get; }
  ColorType ColorType { get; }
  float MinLod => -1000;
  float MaxLod => 1000;
  float LodBias => 0;
}

public record GxTexture2d(
    string Name,
    IReadOnlyImage[] MipmapImages,
    GxWrapMode WrapModeS,
    GxWrapMode WrapModeT,
    GX_MIN_TEXTURE_FILTER MinTextureFilter
        = GX_MIN_TEXTURE_FILTER.GX_LIN_MIP_LIN,
    GX_MAG_TEXTURE_FILTER MagTextureFilter = GX_MAG_TEXTURE_FILTER.GX_LINEAR,
    ColorType ColorType = ColorType.COLOR,
    float LodBias = 0) : IGxTexture;