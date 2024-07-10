using fin.image;

namespace fin.model.impl;

public partial class ModelImpl<TVertex> {
  private partial class MaterialManagerImpl {
    public IScrollingTexture CreateScrollingTexture(
        IReadOnlyImage imageData,
        float scrollSpeedX,
        float scrollSpeedY) {
      var texture = new ScrollingTextureImpl(this.textures_.Count,
                                             imageData,
                                             scrollSpeedX,
                                             scrollSpeedY);
      this.textures_.Add(texture);
      return texture;
    }
  }

  private class ScrollingTextureImpl : BTextureImpl, IScrollingTexture {
    public ScrollingTextureImpl(
        int index,
        IReadOnlyImage image,
        float scrollSpeedX,
        float scrollSpeedY) : base(index, [image]) {
      this.ScrollSpeedX = scrollSpeedX;
      this.ScrollSpeedY = scrollSpeedY;
    }

    public float ScrollSpeedX { get; }
    public float ScrollSpeedY { get; }
  }
}