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
            .LINEAR_MIPMAP_NEAR,
        GX_MIN_TEXTURE_FILTER.GX_NEAR2 => TextureMinFilter.NEAR,
        GX_MIN_TEXTURE_FILTER.GX_NEAR3 => TextureMinFilter.NEAR,
    };
};

public interface IGxTexture {
  string Name { get; }
  IImage Image { get; }
  GxWrapMode WrapModeS { get; }
  GxWrapMode WrapModeT { get; }
  GX_MAG_TEXTURE_FILTER MagTextureFilter { get; }
  GX_MIN_TEXTURE_FILTER MinTextureFilter { get; }
  ColorType ColorType { get; }
}

public class GxTexture2d : IGxTexture {
  public string Name { get; set; }
  public IImage Image { get; set; }
  public GxWrapMode WrapModeS { get; set; }
  public GxWrapMode WrapModeT { get; set; }

  public GX_MIN_TEXTURE_FILTER MinTextureFilter { get; set; } =
    GX_MIN_TEXTURE_FILTER.GX_LIN_MIP_LIN;

  public GX_MAG_TEXTURE_FILTER MagTextureFilter { get; set; } =
    GX_MAG_TEXTURE_FILTER.GX_LINEAR;

  public ColorType ColorType { get; set; }
}