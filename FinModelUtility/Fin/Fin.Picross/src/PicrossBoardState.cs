using fin.data;
using fin.util.asserts;

namespace fin.picross;

public enum PicrossCellState {
  UNKNOWN,
  KNOWN_EMPTY,
  KNOWN_FILLED,
}

public class PicrossBoardState(
    IPicrossDefinition picrossDefinition,
    PicrossCellState[] states) : IReadOnlyGrid<PicrossCellState> {
  public PicrossBoardState(IPicrossDefinition picrossDefinition) : this(
      picrossDefinition,
      new PicrossCellState[picrossDefinition.Width *
                           picrossDefinition.Height]) { }

  public int Width => picrossDefinition.Width;
  public int Height => picrossDefinition.Height;

  public PicrossCellState this[int x, int y] => states[y * this.Width + x];

  public PicrossBoardState ApplyMoves(IReadOnlySet<PicrossMove> moveSet) {
    var width = this.Width;
    var newStates = states.ToArray();
    foreach (var move in moveSet) {
      var (moveType, _, x, y) = move;

      // Verifies the board didn't already have a move at this location.
      Asserts.Equal(PicrossCellState.UNKNOWN, states[y * width + x]);

      // Applies the move to the cell.
      newStates[y * width + x] = moveType switch {
          PicrossMoveType.MARK_EMPTY  => PicrossCellState.KNOWN_EMPTY,
          PicrossMoveType.MARK_FILLED => PicrossCellState.KNOWN_FILLED,
          _                           => throw new ArgumentOutOfRangeException()
      };
    }

    return new PicrossBoardState(picrossDefinition, newStates);
  }

  public bool IsSolved() {
    var width = this.Width;
    for (var y = 0; y < this.Height; ++y) {
      for (var x = 0; x < width; ++x) {
        if (!picrossDefinition[x, y]) {
          continue;
        }

        if (states[y * width + x] != PicrossCellState.KNOWN_FILLED) {
          return false;
        }
      }
    }

    return true;
  }
}