using fin.math;
using fin.picross.moves;
using fin.util.enumerables;

namespace fin.picross.solver;

public class BigThenSinglesSolverMethod : BBidirectionalSolverMethod {
  public override IEnumerable<IPicrossMove1d> TryToFindMoves(
      IPicrossLineState lineState,
      int iStart,
      int iEnd,
      int clueStart,
      int clueEnd,
      int increment) {
    var clues = lineState.ClueStates;
    var cellStates = lineState.CellStates;
    var length = cellStates.Count;

    if (clues.All(c => c.Solved)) {
      yield break;
    }

    // Move past all starting solved clues in line
    while (clues[clueStart].Solved) {
      var clue = clues[clueStart];
      clueStart += increment;

      if (increment == 1) {
        iStart = clue.StartIndex.Value + clue.Length;
      } else {
        iStart = clue.StartIndex.Value + increment;
      }
    }

    // Verify next clue is big
    var bigUnclaimedClue = clues[clueStart];
    if (bigUnclaimedClue.Length == 1) {
      yield break;
    }

    clueStart += increment;

    // Verify remaining unsolved clues are singles
    var unsolvedSingleCount = 0;
    for (var i = clueStart; i != clueEnd; i += increment) {
      var clue = clues[i];
      if (clue.Solved) {
        continue;
      }

      if (clue.Length == 1) {
        ++unsolvedSingleCount;
      } else {
        yield break;
      }
    }

    if (unsolvedSingleCount == 0) {
      yield break;
    }

    // Fill in empties around singles
    var inUnclaimedClue = false;
    int? firstBigUnclaimedCell = null;
    for (var i = iStart; i != iEnd; i += increment) {
      var cellState = cellStates[i];
      var isUnclaimed
          = cellState.Status == PicrossCellStatus.KNOWN_FILLED &&
            (lineState.IsColumn
                ? cellState.ColumnClue == null
                : cellState.RowClue == null);

      if (isUnclaimed && !inUnclaimedClue && firstBigUnclaimedCell == null) {
        firstBigUnclaimedCell = i;
      }

      var totalLength = increment == 1
          ? (i - firstBigUnclaimedCell + 1)
          : (firstBigUnclaimedCell - i + 1);

      if (isUnclaimed && totalLength > bigUnclaimedClue.Length) {
        var iBefore = i - increment;
        var iAfter = i + increment;

        if (iBefore.IsInRange(0, length - 1) &&
            cellStates[iBefore].Status == PicrossCellStatus.UNKNOWN) {
          yield return new PicrossCellMove1d(
              PicrossCellMoveType.MARK_EMPTY,
              PicrossCellMoveSource.SINGLE_AFTER_BIG_CLUE,
              iBefore);
        }

        if (iAfter.IsInRange(0, length - 1) &&
            cellStates[iAfter].Status == PicrossCellStatus.UNKNOWN) {
          yield return new PicrossCellMove1d(
              PicrossCellMoveType.MARK_EMPTY,
              PicrossCellMoveSource.SINGLE_AFTER_BIG_CLUE,
              iAfter);
        }
      }

      inUnclaimedClue = isUnclaimed;
    }
  }
}