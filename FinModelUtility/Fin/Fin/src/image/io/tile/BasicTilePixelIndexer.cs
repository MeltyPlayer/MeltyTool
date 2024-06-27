namespace fin.image.io.tile;

/// <summary>
///   Basic pixels indexer, where it assumes the pixels are laid out row-wise.
/// </summary>
public class BasicPixelIndexer : IPixelIndexer {
  private readonly int width_;

  public BasicPixelIndexer(int width) {
    this.width_ = width;
  }

  public void GetPixelCoordinates(int index, out int x, out int y) {
    x = index % this.width_;
    y = index / this.width_;
  }
}