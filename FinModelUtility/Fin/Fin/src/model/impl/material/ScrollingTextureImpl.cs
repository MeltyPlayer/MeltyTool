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

  private class ScrollingTextureImpl(
      int index,
      IReadOnlyImage image,
      float scrollSpeedX,
      float scrollSpeedY)
      : BTextureImpl(index, [image]), IScrollingTexture {
    public float ScrollSpeedX { get; } = scrollSpeedX;
    public float ScrollSpeedY { get; } = scrollSpeedY;
  }
}