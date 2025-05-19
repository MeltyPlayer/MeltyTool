using fin.picross;
using fin.util.asserts;

namespace MariosPicross;

public class PicrossDefinition30X30 : IPicrossDefinition {
  private readonly IPicrossDefinition[] picrossDefinitions_;

  public PicrossDefinition30X30(
      ReadOnlySpan<IPicrossDefinition> picrossDefinitions) {
    var topLeft = picrossDefinitions[0];
    var topRight = picrossDefinitions[1];
    var bottomRight = picrossDefinitions[2];
    var bottomLeft = picrossDefinitions[3];

    this.picrossDefinitions_ = [topLeft, topRight, bottomLeft, bottomRight];
    foreach (var picrossDefinition in this.picrossDefinitions_) {
      Asserts.Equal(15, picrossDefinition.Width);
      Asserts.Equal(15, picrossDefinition.Height);
    }
  }

  public string Name { get; set; }
  public byte Width => 30;
  public byte Height => 30;

  public bool this[int x, int y] {
    get {
      GetPicrossDefinitionIndexAndCoords_(
          x,
          y,
          out var picrossDefinitionIndex,
          out var picrossX,
          out var picrossY);
      return this.picrossDefinitions_[picrossDefinitionIndex][picrossX, picrossY];
    }
  }

  private static void GetPicrossDefinitionIndexAndCoords_(
      int x,
      int y,
      out int picrossDefinitionIndex,
      out int picrossX,
      out int picrossY) {
    var picrossXIndex = (int) MathF.Floor(x / 15f);
    var picrossYIndex = (int) MathF.Floor(y / 15f);
    picrossDefinitionIndex = picrossYIndex * 2 + picrossXIndex;

    picrossX = (byte) (x - 15 * picrossXIndex);
    picrossY = (byte) (y - 15 * picrossYIndex);
  }
}