using fin.animation.interpolation;
using fin.data.indexable;

namespace fin.model.impl;

public partial class ModelImpl<TVertex> {
  private partial class ModelAnimationImpl {
    private readonly IndexableDictionary<IReadOnlyTexture, ITextureTracks>
        textureTracks_ = new();

    public IReadOnlyIndexableDictionary<IReadOnlyTexture, ITextureTracks>
        TextureTracks => this.textureTracks_;

    public ITextureTracks AddTextureTracks(IReadOnlyTexture texture)
      => this.textureTracks_[texture]
          = new TextureTracksImpl(texture, this.sharedInterpolationConfig_);
  }

  private partial class TextureTracksImpl(
      IReadOnlyTexture texture,
      ISharedInterpolationConfig sharedConfig)
      : ITextureTracks {
    public IReadOnlyTexture Texture => texture;
  }
}