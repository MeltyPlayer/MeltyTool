using fin.image;
using fin.io;

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

  public static IPicrossDefinition FromImageFile(IReadOnlyTreeFile imageFile) {
    using var image = FinImage.FromFile(imageFile);

    var picrossDefinition = new PicrossDefinition(image.Width, image.Height);
    picrossDefinition.Name = imageFile.NameWithoutExtension.ToString();

    image.Access(getHandler => {
      for (var y = 0; y < picrossDefinition.Height; ++y) {
        for (var x = 0; x < picrossDefinition.Width; ++x) {
          getHandler(x, y, out var r, out _, out _, out _);
          picrossDefinition[x, y] = r < 128;
        }
      }
    });

    return picrossDefinition;
  }
}