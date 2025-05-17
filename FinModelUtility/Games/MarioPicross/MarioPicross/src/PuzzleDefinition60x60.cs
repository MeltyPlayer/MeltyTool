using fin.util.asserts;

namespace MariosPicross;

public class PuzzleDefinition60x60 : IPuzzleDefinition {
  private readonly IPuzzleDefinition[] puzzleDefinitions_;

  public PuzzleDefinition60x60(
      ReadOnlySpan<IPuzzleDefinition> puzzleDefinitions) {
    var topLeft = puzzleDefinitions[0];
    var topRight = puzzleDefinitions[1];
    var bottomLeft = puzzleDefinitions[2];
    var bottomRight = puzzleDefinitions[3];

    this.puzzleDefinitions_ = [topLeft, topRight, bottomLeft, bottomRight];
    foreach (var puzzleDefinition in this.puzzleDefinitions_) {
      Asserts.IsA<PuzzleDefinition30x30>(puzzleDefinition);
    }
  }

  public string Name { get; set; }
  public byte Width => 60;
  public byte Height => 60;

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
    var puzzleXIndex = (int) MathF.Floor(x / 30f);
    var puzzleYIndex = (int) MathF.Floor(y / 30f);
    puzzleDefinitionIndex = puzzleYIndex * 2 + puzzleXIndex;

    puzzleX = (byte) (x - 30 * puzzleXIndex);
    puzzleY = (byte) (y - 30 * puzzleYIndex);
  }
}