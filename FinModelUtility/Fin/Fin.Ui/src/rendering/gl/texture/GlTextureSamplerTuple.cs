using System.Runtime.CompilerServices;

using fin.data;
using fin.model;

using TextureMinFilter = fin.model.TextureMinFilter;

namespace fin.ui.rendering.gl.texture;

public sealed record GlTextureSamplerTupleParams {
  public required GlTextureParams TextureParams { get; init; }
  public required GlSamplerParams SamplerParams { get; init; }
}

public sealed class GlTextureSamplerTuple : IGlTexture {
  private static ReferenceCountCacheDictionary<GlTextureSamplerTupleParams,
          GlTextureSamplerTuple>
      cache_ = new(
          prms => new GlTextureSamplerTuple(
              prms,
              GlTexture.FromParams(prms.TextureParams),
              GlSampler.FromParams(prms.SamplerParams)),
          (_, glTextureSamplerTuple) => {
            glTextureSamplerTuple.texture_.Dispose();
            glTextureSamplerTuple.sampler_.Dispose();
            glTextureSamplerTuple.IsDisposed = true;
          });

  private readonly GlTextureSamplerTupleParams params_;
  private readonly GlTexture texture_;
  private readonly GlSampler sampler_;

  public static GlTextureSamplerTuple FromTexture(IReadOnlyTexture texture) {
    return cache_.GetAndIncrement(new GlTextureSamplerTupleParams {
        TextureParams = GlTextureParams.FromTexture(texture),
        SamplerParams = GlSamplerParams.FromTexture(texture),
    });
  }

  public GlTextureSamplerTuple(
      GlTextureSamplerTupleParams prms,
      GlTexture texture,
      GlSampler sampler) {
    this.params_ = prms;
    this.texture_ = texture;
    this.sampler_ = sampler;

    if (prms.SamplerParams is {
            ThreePointFiltering: false,
            MinFilter: TextureMinFilter.NEAR_MIPMAP_NEAR
                       or TextureMinFilter.NEAR_MIPMAP_LINEAR
                       or TextureMinFilter.LINEAR_MIPMAP_NEAR
                       or TextureMinFilter.LINEAR_MIPMAP_LINEAR
        }) {
      texture.GenerateMipmapsIfHaventYet();
    }
  }

  ~GlTextureSamplerTuple() => this.ReleaseUnmanagedResources_();

  public bool IsDisposed { get; private set; }

  public void Dispose() {
    this.ReleaseUnmanagedResources_();
    GC.SuppressFinalize(this);
  }

  private void ReleaseUnmanagedResources_() {
    if (this.IsDisposed) {
      return;
    }

    cache_.DecrementAndMaybeDispose(this.params_);
  }

  public int Id => this.texture_.Id;

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public void Bind(int textureIndex = 0) {
    this.texture_.Bind(textureIndex);
    this.sampler_.Bind(textureIndex);
  }
}