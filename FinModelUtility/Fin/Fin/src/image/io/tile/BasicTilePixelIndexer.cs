namespace fin.image.io.tile;

/// <summary>
///   Basic pixels indexer, where it assumes the pixels are laid out row-wise.
/// </summary>
public sealed class BasicPixelIndexer(int width) : IPixelIndexer {
  public void GetPixelCoordinates(int index, out int x, out int y) {
    x = index % width;
    y = index / width;
  }
}