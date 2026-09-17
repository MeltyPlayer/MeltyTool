using System.Runtime.CompilerServices;

using fin.color;
using fin.data;
using fin.math.floats;
using fin.model;

using OpenTK.Graphics.OpenGL4;

using FinTextureMinFilter = fin.model.TextureMinFilter;
using TextureMagFilter = fin.model.TextureMagFilter;
using TextureMinFilter = OpenTK.Graphics.OpenGL4.TextureMinFilter;


namespace fin.ui.rendering.gl.texture;

public sealed record GlSamplerParams {
  public static GlSamplerParams FromTexture(IReadOnlyTexture texture)
    => new() {
        WrapModeU = texture.WrapModeU,
        WrapModeV = texture.WrapModeV,

        MinFilter = texture.MinFilter,
        MagFilter = texture.MagFilter,
        BorderColor = texture.BorderColor,

        MinLod = texture.MinLod,
        MaxLod = texture.MaxLod,
        LodBias = texture.LodBias,

        ThreePointFiltering = texture.ThreePointFiltering,    };

  public WrapMode WrapModeU { get; init; }
  public WrapMode WrapModeV { get; init; }

  public required FinTextureMinFilter MinFilter { get; init; }
  public required TextureMagFilter MagFilter { get; init; }
  public required IColor? BorderColor { get; init; }

  public required float MinLod { get; init; }
  public required float MaxLod { get; init; }
  public required float LodBias { get; init; }

  public required bool ThreePointFiltering { get; init; }
}

public sealed class GlSampler : IGlSampler {
  // Intentionally separates params from texture, so we can share a single GL
  // texture between multiple Fin textures.
  private static ReferenceCountCacheDictionary<GlSamplerParams, GlSampler>
      cache_ = new(
          prms => new GlSampler(prms),
          (_, GlSampler) => {
            var id = GlSampler.Id;
            if (id != UNDEFINED_ID) {
              GL.DeleteSamplers(1, ref id);
              GlSampler.Id = id;
              GlSampler.IsDisposed = true;
            }
          },
          count => DebugService.SamplerCount = count);

  private const int UNDEFINED_ID = -1;
  private readonly GlSamplerParams? params_;

  public static GlSampler FromTexture(IReadOnlyTexture texture)
    => FromParams(GlSamplerParams.FromTexture(texture));

  public static GlSampler FromParams(GlSamplerParams prms)
    => cache_.GetAndIncrement(prms);


  private GlSampler(GlSamplerParams prms) {
    this.params_ = prms;

    FinTextureMinFilter minFilter;
    TextureMagFilter magFilter;
    if (!prms.ThreePointFiltering) {
      minFilter = prms.MinFilter;
      magFilter = prms.MagFilter;
    } else {
      // TODO: This is just an assumption for now, what should this be?
      minFilter = FinTextureMinFilter.NEAR;
      magFilter = TextureMagFilter.NEAR;
    }

    GL.GenSamplers(1, out int id);
    this.Id = id;

    {
      var finBorderColor = prms.BorderColor;
      var hasBorderColor = finBorderColor != null;
      GL.SamplerParameter(
          id,
          SamplerParameterName.TextureWrapS,
          (int) ConvertFinWrapToGlWrap_(
              prms.WrapModeU,
              hasBorderColor));
      GL.SamplerParameter(id,
                          SamplerParameterName.TextureWrapT,
                          (int) ConvertFinWrapToGlWrap_(
                              prms.WrapModeV,
                              hasBorderColor));

      if (hasBorderColor) {
        var glBorderColor = new[] {
            finBorderColor.Rf,
            finBorderColor.Gf,
            finBorderColor.Bf,
            finBorderColor.Af
        };

        GL.SamplerParameter(id,
                            SamplerParameterName.TextureBorderColor,
                            glBorderColor);
      }

      GL.SamplerParameter(
          id,
          SamplerParameterName.TextureMinFilter,
          (int) (minFilter switch {
              FinTextureMinFilter.NEAR   => TextureMinFilter.Nearest,
              FinTextureMinFilter.LINEAR => TextureMinFilter.Linear,
              FinTextureMinFilter.NEAR_MIPMAP_NEAR => TextureMinFilter
                  .NearestMipmapNearest,
              FinTextureMinFilter.NEAR_MIPMAP_LINEAR => TextureMinFilter
                  .NearestMipmapLinear,
              FinTextureMinFilter.LINEAR_MIPMAP_NEAR => TextureMinFilter
                  .LinearMipmapNearest,
              FinTextureMinFilter.LINEAR_MIPMAP_LINEAR => TextureMinFilter
                  .LinearMipmapLinear,
          }));
      GL.SamplerParameter(
          id,
          SamplerParameterName.TextureMagFilter,
          (int) (magFilter switch {
              TextureMagFilter.NEAR => OpenTK.Graphics.OpenGL.TextureMagFilter
                                             .Nearest,
              TextureMagFilter.LINEAR => OpenTK.Graphics.OpenGL
                                               .TextureMagFilter.Linear,
              _ => throw new ArgumentOutOfRangeException()
          }));
      GL.SamplerParameter(id,
                          SamplerParameterName.TextureMinLod,
                          prms.MinLod);
      GL.SamplerParameter(id,
                          SamplerParameterName.TextureMaxLod,
                          prms.MaxLod);
      if (!prms.LodBias.IsRoughly0()) {
        GL.SamplerParameter(id,
                            SamplerParameterName.TextureLodBias,
                            prms.LodBias);
      }
    }
  }

  ~GlSampler() => this.ReleaseUnmanagedResources_();

  public bool IsDisposed { get; private set; }

  public void Dispose() {
    this.ReleaseUnmanagedResources_();
    GC.SuppressFinalize(this);
  }

  private void ReleaseUnmanagedResources_() {
    if (this.IsDisposed) {
      return;
    }

    if (this.params_ != null) {
      cache_.DecrementAndMaybeDispose(this.params_);
    }
  }

  public int Id { get; private set; } = UNDEFINED_ID;

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public void Bind(int samplerIndex = 0)
    => GlUtil.BindSampler(samplerIndex, this.Id);

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