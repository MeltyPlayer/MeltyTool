using fin.math.floats;
using fin.model;

using OpenTK.Graphics.OpenGL4;

using FinTextureMinFilter = fin.model.TextureMinFilter;
using FinTextureMagFilter = fin.model.TextureMagFilter;
using GlTextureMinFilter = OpenTK.Graphics.OpenGL4.TextureMinFilter;
using GlTextureMagFilter = OpenTK.Graphics.OpenGL4.TextureMagFilter;

namespace fin.ui.rendering.gl.texture;

public static class GlSamplerUtil {
  public readonly struct OpenGlSamplerParams {
    public required int WrapS { get; init; }
    public required int WrapT { get; init; }
    public required float[]? BorderColor { get; init; }

    public required GlTextureMinFilter MinFilter { get; init; }
    public required GlTextureMagFilter MagFilter { get; init; }

    public required float MinLod { get; init; }
    public required float MaxLod { get; init; }
    public required float? LodBias { get; init; }
  }

  public static OpenGlSamplerParams GetOpenGlSamplerParams(GlSamplerParams prms) {
    var finBorderColor = prms.BorderColor;
    var hasBorderColor = finBorderColor != null;

    var glWrapS = ConvertFinWrapToGlWrap_(
        prms.WrapModeU,
        hasBorderColor);
    var glWrapT = ConvertFinWrapToGlWrap_(
        prms.WrapModeV,
        hasBorderColor);
    float[]? glBorderColor = null;
    if (hasBorderColor) {
      glBorderColor = [
          finBorderColor.Rf,
          finBorderColor.Gf,
          finBorderColor.Bf,
          finBorderColor.Af
      ];
    }

    FinTextureMinFilter minFilter;
    FinTextureMagFilter magFilter;
    if (!prms.ThreePointFiltering) {
      minFilter = prms.MinFilter;
      magFilter = prms.MagFilter;
    } else {
      // TODO: This is just an assumption for now, what should this be?
      minFilter = FinTextureMinFilter.NEAR;
      magFilter = FinTextureMagFilter.NEAR;
    }

    var glMinFilter = minFilter switch {
        FinTextureMinFilter.NEAR   => GlTextureMinFilter.Nearest,
        FinTextureMinFilter.LINEAR => GlTextureMinFilter.Linear,
        FinTextureMinFilter.NEAR_MIPMAP_NEAR => GlTextureMinFilter
            .NearestMipmapNearest,
        FinTextureMinFilter.NEAR_MIPMAP_LINEAR => GlTextureMinFilter
            .NearestMipmapLinear,
        FinTextureMinFilter.LINEAR_MIPMAP_NEAR => GlTextureMinFilter
            .LinearMipmapNearest,
        FinTextureMinFilter.LINEAR_MIPMAP_LINEAR => GlTextureMinFilter
            .LinearMipmapLinear,
    };
    var glMagFilter = magFilter switch {
        FinTextureMagFilter.NEAR   => GlTextureMagFilter.Nearest,
        FinTextureMagFilter.LINEAR => GlTextureMagFilter.Linear,
        _                          => throw new ArgumentOutOfRangeException()
    };

    return new OpenGlSamplerParams {
        WrapS = glWrapS,
        WrapT = glWrapT,
        BorderColor = glBorderColor,

        MinFilter = glMinFilter,
        MagFilter = glMagFilter,

        MinLod = prms.MinLod,
        MaxLod = prms.MaxLod,
        LodBias = !prms.LodBias.IsRoughly0() ? prms.LodBias : null,
    };
  }

  private static int ConvertFinWrapToGlWrap_(
      WrapMode wrapMode,
      bool hasBorderColor) =>
      wrapMode switch {
          WrapMode.CLAMP => hasBorderColor
              ? (int) TextureWrapMode.ClampToBorder
              : (int) TextureWrapMode.ClampToEdge,
          WrapMode.REPEAT        => (int) TextureWrapMode.Repeat,
          WrapMode.MIRROR_CLAMP  => (int) All.MirrorClampToEdge,
          WrapMode.MIRROR_REPEAT => (int) All.MirroredRepeat,
          _ => throw new ArgumentOutOfRangeException(
              nameof(wrapMode),
              wrapMode,
              null)
      };
}