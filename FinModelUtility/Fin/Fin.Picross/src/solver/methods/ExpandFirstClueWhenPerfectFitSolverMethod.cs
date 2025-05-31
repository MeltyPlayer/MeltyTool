using fin.picross.moves;

namespace fin.picross.solver.methods;

public class ExpandFirstClueWhenPerfectFitSolverMethod
    : BBidirectionalSolverMethod {
  public override IEnumerable<IPicrossMove1d> TryToFindMoves(
      IPicrossLineState lineState,
      int iStart,
      int iEnd,
      int clueStart,
      int clueEnd,
      int increment) {
    var clues = lineState.ClueStates;
    var cellStates = lineState.CellStates;

    if (clues.All(c => c.Solved)) {
      yield break;
    }

    PicrossSkipUtil.SkipSolvedClues(
        lineState,
        increment,
        ref clueStart,
        ref iStart);

    PicrossSkipUtil.SkipEmpty(lineState, increment, ref iStart);

    var firstClue = clues[clueStart];

    var foundFirstClue = false;
    for (var i = iStart; i != iEnd; i += increment) {
      var cellState = cellStates[i];
      var isUnclaimed
          = cellState.Status == PicrossCellStatus.KNOWN_FILLED &&
            (lineState.IsColumn
                ? cellState.ColumnClue == null
                : cellState.RowClue == null);
      if (isUnclaimed) {
        foundFirstClue = true;
      }

      // Found end of gap. Hopefully the first clue fits!
      if (cellState.Status == PicrossCellStatus.KNOWN_EMPTY) {
        if (!foundFirstClue) {
          yield break;
        }

        var absoluteStartI = increment == 1 ? iStart : i - increment;
        var absoluteLength = increment == 1 ? i : iStart - increment;
        var gapLength = Math.Abs(i - iStart);

        if (firstClue.Length == gapLength ||
            firstClue.Length + 1 == gapLength) {
          foreach (var move in PicrossMultiMoveUtil.AlignGapToClue(
                       lineState,
                       firstClue.Length,
                       absoluteStartI,
                       absoluteLength)) {
            yield return move;
          }
        }
      }
    }
  }
}