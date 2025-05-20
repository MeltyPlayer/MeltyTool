namespace fin.picross;

public class PicrossDefinition(int width, int height) : IPicrossDefinition {
  private readonly bool[] impl_ = new bool[width * height];

  public string Name { get; set; }

  public int Width => width;
  public int Height => height;

  public bool this[int x, int y] {
    get => this.impl_[y * width + x];
    set => this.impl_[y * width + x] = value;
  }
}