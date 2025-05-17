using fin.util.asserts;

namespace MariosPicross;

public class PicrossDefinition60X60 : IPicrossDefinition {
  private readonly IPicrossDefinition[] picrossDefinitions_;

  public PicrossDefinition60X60(
      ReadOnlySpan<IPicrossDefinition> picrossDefinitions) {
    var topLeft = picrossDefinitions[0];
    var topRight = picrossDefinitions[1];
    var bottomLeft = picrossDefinitions[2];
    var bottomRight = picrossDefinitions[3];

    this.picrossDefinitions_ = [topLeft, topRight, bottomLeft, bottomRight];
    foreach (var picrossDefinition in this.picrossDefinitions_) {
      Asserts.IsA<PicrossDefinition30X30>(picrossDefinition);
    }
  }

  public string Name { get; set; }
  public byte Width => 60;
  public byte Height => 60;

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
    var picrossXIndex = (int) MathF.Floor(x / 30f);
    var picrossYIndex = (int) MathF.Floor(y / 30f);
    picrossDefinitionIndex = picrossYIndex * 2 + picrossXIndex;

    picrossX = (byte) (x - 30 * picrossXIndex);
    picrossY = (byte) (y - 30 * picrossYIndex);
  }
}