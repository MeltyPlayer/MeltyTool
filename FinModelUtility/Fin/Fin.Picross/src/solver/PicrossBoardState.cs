using fin.data;
using fin.data.indexable;
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
  IPicrossClue? ColumnClue { get; set; }
  IPicrossClue? RowClue { get; set; }
}

public class PicrossCellState : IPicrossCellState {
  public PicrossCellStatus Status { get; set; }
  public PicrossCellMoveSource MoveSource { get; set; }
  public IPicrossClue? ColumnClue { get; set; }
  public IPicrossClue? RowClue { get; set; }
}

public interface IPicrossBoardState : IReadOnlyGrid<IReadOnlyPicrossCellState> {
  IReadOnlyList<IPicrossLineState> ColumnLineStates { get; }
  IReadOnlyList<IPicrossLineState> RowLineStates { get; }
}

public class PicrossBoardState : IReadOnlyGrid<IReadOnlyPicrossCellState> {
  private readonly IPicrossDefinition definition_;
  private readonly IReadOnlyList<IPicrossCellState> cellStates_;

  private readonly IReadOnlyIndexableDictionary<IPicrossClue, IPicrossClueState>
      clueStateByClue_;

  private readonly IReadOnlyList<IPicrossLineState> columnLineStates_;
  private readonly IReadOnlyList<IPicrossLineState> rowLineStates_;

  public PicrossBoardState(IPicrossDefinition definition) {
    this.definition_ = definition;

    var cellStates
        = new IPicrossCellState[definition.Width * definition.Height];
    this.cellStates_ = cellStates;
    for (var i = 0; i < cellStates.Length; ++i) {
      cellStates[i] = new PicrossCellState();
    }

    var clues = new PicrossCluesGenerator().GenerateClues(definition);

    var columnClueStates = ToClueStates_(clues.Columns);
    this.columnLineStates_
        = columnClueStates
          .Select((clues, x) => new PicrossLineState {
              IsColumn = true,
              ClueStates = clues,
              CellStates = this.GetColumn(x).ToArray(),
          })
          .ToArray();
    var rowClueStates = ToClueStates_(clues.Rows);
    this.rowLineStates_
        = rowClueStates
          .Select((clues, y) => new PicrossLineState {
              IsColumn = false,
              ClueStates = clues,
              CellStates = this.GetRow(y).ToArray(),
          })
          .ToArray();

    var clueStatesByClue
        = new IndexableDictionary<IPicrossClue, IPicrossClueState>();
    this.clueStateByClue_ = clueStatesByClue;
    foreach (var clueState in columnClueStates.Concat(rowClueStates)
                                              .SelectMany(c => c)) {
      clueStatesByClue[clueState.Clue] = clueState;
    }
  }

  private static IReadOnlyList<IReadOnlyList<IPicrossClueState>> ToClueStates_(
      IReadOnlyList<IReadOnlyList<IPicrossClue>> clues)
    => clues.Select(t => t.Select(v => new PicrossClueState(v)).ToArray())
            .ToArray();

  public int Width => this.definition_.Width;
  public int Height => this.definition_.Height;

  public IReadOnlyPicrossCellState this[int x, int y]
    => this.cellStates_[y * this.Width + x];

  public IReadOnlyList<IPicrossLineState> ColumnLineStates
    => this.columnLineStates_;
  public IReadOnlyList<IPicrossLineState> RowLineStates => this.rowLineStates_;

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
          var (clueMoveSource, clue, startI) = picrossClueMove;
          var clueState = this.clueStateByClue_[clue];

          // Verifies clue is at the correct location.
          Asserts.Equal(clue.CorrectStartIndex,
                        startI,
                        $"Incorrect clue move of source {clueMoveSource}; marked as starting at {startI} but should actually be {clue.CorrectStartIndex}");

          // Verifies we didn't already mark this clue as solved.
          Asserts.False(clueState.Solved,
                        $"Got duplicate clue solution of source {clueMoveSource}");

          clueState.StartIndex = startI;

          for (var i = startI; i < startI + clue.Length; ++i) {
            if (clue.IsForColumn) {
              var x = clue.ColumnOrRowIndex;
              var y = i;
              this.cellStates_[y * width + x].ColumnClue = clue;
            } else {
              var x = i;
              var y = clue.ColumnOrRowIndex;
              this.cellStates_[y * width + x].RowClue = clue;
            }
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