using fin.image;

namespace fin.model.impl;

public partial class ModelImpl<TVertex> {
  private partial class MaterialManagerImpl {
    public ITexture CreateTexture(IReadOnlyImage imageData) {
      var texture = new TextureImpl(imageData);
      this.textures_.Add(texture);
      return texture;
    }
  }

  private class TextureImpl : BTextureImpl {
    public TextureImpl(IReadOnlyImage image) : base(image) { }
  }
}