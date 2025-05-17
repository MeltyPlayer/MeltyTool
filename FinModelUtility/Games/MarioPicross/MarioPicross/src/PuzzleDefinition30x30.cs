using fin.util.asserts;

namespace MariosPicross;

public class PuzzleDefinition30x30 : IPuzzleDefinition {
  private readonly IPuzzleDefinition[] puzzleDefinitions_;

  public PuzzleDefinition30x30(
      ReadOnlySpan<IPuzzleDefinition> puzzleDefinitions) {
    var topLeft = puzzleDefinitions[0];
    var topRight = puzzleDefinitions[1];
    var bottomRight = puzzleDefinitions[2];
    var bottomLeft = puzzleDefinitions[3];

    this.puzzleDefinitions_ = [topLeft, topRight, bottomLeft, bottomRight];
    foreach (var puzzleDefinition in this.puzzleDefinitions_) {
      Asserts.Equal(15, puzzleDefinition.Width);
      Asserts.Equal(15, puzzleDefinition.Height);
    }
  }

  public string Name { get; set; }
  public byte Width => 30;
  public byte Height => 30;

  public bool this[int x, int y] {
    get {
      GetPuzzleDefinitionIndexAndCoords_(
          x,
          y,
          out var puzzleDefinitionIndex,
          out var puzzleX,
          out var puzzleY);
      return this.puzzleDefinitions_[puzzleDefinitionIndex][puzzleX, puzzleY];
    }
  }

  private static void GetPuzzleDefinitionIndexAndCoords_(
      int x,
      int y,
      out int puzzleDefinitionIndex,
      out int puzzleX,
      out int puzzleY) {
    var puzzleXIndex = (int) MathF.Floor(x / 15f);
    var puzzleYIndex = (int) MathF.Floor(y / 15f);
    puzzleDefinitionIndex = puzzleYIndex * 2 + puzzleXIndex;

    puzzleX = (byte) (x - 15 * puzzleXIndex);
    puzzleY = (byte) (y - 15 * puzzleYIndex);
  }
}