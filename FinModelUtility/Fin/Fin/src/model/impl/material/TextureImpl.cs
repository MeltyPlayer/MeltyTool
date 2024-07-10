using fin.image;
using fin.util.asserts;

namespace fin.model.impl;

public partial class ModelImpl<TVertex> {
  private partial class MaterialManagerImpl {
    public ITexture CreateTexture(IReadOnlyImage image)
      => this.CreateTexture([image]);

    public ITexture CreateTexture(IReadOnlyImage[] mipmapImages) {
      Asserts.True(mipmapImages.Length >= 1,
                   "Expected texture to have at least 1 mipmap!");

      var texture = new TextureImpl(this.textures_.Count, mipmapImages);
      this.textures_.Add(texture);
      return texture;
    }
  }

  private class TextureImpl(int index, IReadOnlyImage[] mipmapImages)
      : BTextureImpl(index, mipmapImages);
}