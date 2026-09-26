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

        ThreePointFiltering = texture.ThreePointFiltering,
    };

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
  private readonly GlSamplerParams params_;

  public static GlSampler FromTexture(IReadOnlyTexture texture)
    => FromParams(GlSamplerParams.FromTexture(texture));

  public static GlSampler FromParams(GlSamplerParams prms)
    => cache_.GetAndIncrement(prms);

  private GlSampler(GlSamplerParams prms) {
    this.params_ = prms;

    GL.GenSamplers(1, out int id);
    this.Id = id;

    var openGlSamplerParams = GlSamplerUtil.GetOpenGlSamplerParams(prms);

    GL.SamplerParameter(
        id,
        SamplerParameterName.TextureWrapS,
        openGlSamplerParams.WrapS);
    GL.SamplerParameter(
        id,
        SamplerParameterName.TextureWrapT,
        openGlSamplerParams.WrapT);

    if (openGlSamplerParams.BorderColor != null) {
      GL.SamplerParameter(
          id,
          SamplerParameterName.TextureBorderColor,
          openGlSamplerParams.BorderColor);
    }

    GL.SamplerParameter(
        id,
        SamplerParameterName.TextureMinFilter,
        (int) openGlSamplerParams.MinFilter);
    GL.SamplerParameter(
        id,
        SamplerParameterName.TextureMagFilter,
        (int) openGlSamplerParams.MagFilter);

    GL.SamplerParameter(
        id,
        SamplerParameterName.TextureMinLod,
        openGlSamplerParams.MinLod);
    GL.SamplerParameter(
        id,
        SamplerParameterName.TextureMaxLod,
        openGlSamplerParams.MaxLod);
    if (openGlSamplerParams.LodBias != null) {
      GL.SamplerParameter(
          id,
          SamplerParameterName.TextureLodBias,
          openGlSamplerParams.LodBias.Value);
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
}