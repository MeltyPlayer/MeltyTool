using fin.data;
using fin.picross.moves;
using fin.util.asserts;

using schema.readOnly;

namespace fin.picross.solver;

public enum PicrossCellStatus {
  UNKNOWN,
  KNOWN_EMPTY,
  KNOWN_FILLED,
}

public enum PicrossCompletionState {
  INCOMPLETE,
  INCORRECT,
  CORRECT,
}

[GenerateReadOnly]
public partial interface IPicrossCellState {
  PicrossCellStatus Status { get; set; }
  PicrossCellMoveSource MoveSource { get; set; }
  IPicrossClue? Clue { get; set; }
}

public class PicrossCellState : IPicrossCellState {
  public PicrossCellStatus Status { get; set; }
  public PicrossCellMoveSource MoveSource { get; set; }
  public IPicrossClue? Clue { get; set; }
}

public class PicrossBoardState : IReadOnlyGrid<IReadOnlyPicrossCellState> {
  private readonly IPicrossDefinition definition_;
  private readonly IPicrossCellState[] cellStates_;

  public PicrossBoardState(IPicrossDefinition definition) {
    this.definition_ = definition;

    this.cellStates_
        = new IPicrossCellState[definition.Width * definition.Height];
    for (var i = 0; i < this.cellStates_.Length; ++i) {
      this.cellStates_[i] = new PicrossCellState();
    }
  }

  public int Width => this.definition_.Width;
  public int Height => this.definition_.Height;

  public IReadOnlyPicrossCellState this[int x, int y]
    => this.cellStates_[y * this.Width + x];

  public void ApplyMoves(IReadOnlySet<IPicrossMove> moveSet) {
    var width = this.Width;
    foreach (var move in moveSet) {
      switch (move) {
        case PicrossCellMove picrossCellMove: {
          var (moveType, moveSource, x, y) = picrossCellMove;

          var cellState = this.cellStates_[y * width + x];

          // Verifies moves are correct against the existing board.
          var expected = moveType == PicrossCellMoveType.MARK_FILLED;
          Asserts.Equal(expected,
                        this.definition_[x, y],
                        $"Incorrect move of source {moveSource}.");

          // Verifies the board didn't already have a move at this location.
          Asserts.Equal(PicrossCellStatus.UNKNOWN,
                        cellState.Status,
                        $"Got a duplicate move of source {moveSource}");

          // Applies the move to the cell.
          cellState.Status = moveType switch {
              PicrossCellMoveType.MARK_EMPTY => PicrossCellStatus.KNOWN_EMPTY,
              PicrossCellMoveType.MARK_FILLED => PicrossCellStatus.KNOWN_FILLED,
              _ => throw new ArgumentOutOfRangeException()
          };
          cellState.MoveSource = moveSource;

          break;
        }
        case PicrossClueMove picrossClueMove: {
          var (_, clue, startI) = picrossClueMove;
          for (var i = startI; i < startI + clue.Length; ++i) {
            int x, y;
            if (clue.IsForColumn) {
              x = clue.ColumnOrRowIndex;
              y = i;
            } else {
              x = i;
              y = clue.ColumnOrRowIndex;
            }

            this.cellStates_[y * width + x].Clue = clue;
          }

          break;
        }
      }
    }
  }

  public PicrossCompletionState GetCompletionState() {
    var incorrectCells = new HashSet<(int, int)>();
    var missingCells = new HashSet<(int, int)>();

    for (var y = 0; y < this.Height; ++y) {
      for (var x = 0; x < this.Width; ++x) {
        var expectedCell = this.definition_[x, y];
        var actualCell = this[x, y].Status;

        if (expectedCell && actualCell != PicrossCellStatus.KNOWN_FILLED) {
          missingCells.Add((x, y));
        }

        if (!expectedCell && actualCell == PicrossCellStatus.KNOWN_FILLED) {
          incorrectCells.Add((x, y));
        }
      }
    }

    return incorrectCells.Count > 0 ? PicrossCompletionState.INCORRECT :
        missingCells.Count > 0 ? PicrossCompletionState.INCOMPLETE :
        PicrossCompletionState.CORRECT;
  }
}