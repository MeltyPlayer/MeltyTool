using fin.math;
using fin.picross.moves;

namespace fin.picross.solver.methods;

public class OnlyClueLengthInReachSolverMethod : BBidirectionalSolverMethod {
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

    var inUnclaimedClue = false;
    var firstUnclaimedCell = -1;

    // Step through, find unclaimed cells
    for (var i = iStart; i != iEnd; i += increment) {
      var cellState = cellStates[i];

      var isUnclaimed
          = cellState.Status == PicrossCellStatus.KNOWN_FILLED &&
            (lineState.IsColumn
              ? cellState.ColumnClue == null
              : cellState.RowClue == null);
      if (isUnclaimed && !inUnclaimedClue) {
        firstUnclaimedCell = i;
      }

      if (!isUnclaimed && inUnclaimedClue) {
        var clueEndIDistance = Math.Abs(i - iStart);
        var clueLength = Math.Abs(firstUnclaimedCell - i);

        // See which clues could fit in up to here, make sure there's only
        // exact matches
        var totalClueDistance = 0;
        for (var clueI = clueStart; clueI != clueEnd; clueI += increment) {
          var clue = clues[clueI];

          totalClueDistance += clue.Length + 1;

          if (!clue.Solved && clue.Length > clueLength) {
            goto NoGoodMatch;
          }

          if (totalClueDistance >= clueEndIDistance) {
            break;
          }
        }

        var beforeI = firstUnclaimedCell - increment;
        var afterI = firstUnclaimedCell + increment * clueLength;

        if (beforeI.IsInRange(0, length - 1) &&
            cellStates[beforeI].Status == PicrossCellStatus.UNKNOWN) {
          yield return new PicrossCellMove1d(
              PicrossCellMoveType.MARK_EMPTY,
              PicrossCellMoveSource.EMPTY_AROUND_ONLY_POSSIBLE_LENGTH,
              beforeI);
        }

        if (afterI.IsInRange(0, length - 1) &&
            cellStates[afterI].Status == PicrossCellStatus.UNKNOWN) {
          yield return new PicrossCellMove1d(
              PicrossCellMoveType.MARK_EMPTY,
              PicrossCellMoveSource.EMPTY_AROUND_ONLY_POSSIBLE_LENGTH,
              afterI);
        }
      }

      NoGoodMatch:

      inUnclaimedClue = isUnclaimed;
    }
  }
}