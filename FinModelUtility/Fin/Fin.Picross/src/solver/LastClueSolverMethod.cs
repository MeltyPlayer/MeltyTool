using fin.picross.moves;
using fin.util.enumerables;

namespace fin.picross.solver;

public class LastClueSolverMethod : IPicrossSolverMethod {
  public IEnumerable<IPicrossMove1d>
      TryToFindMoves(IPicrossLineState lineState) {
    // Make sure we only run this solver if there's a single unsolved clue
    var clueStates = lineState.ClueStates;
    var lastUnsolvedClue
        = clueStates.FirstOrDefaultAndCount(c => !c.Solved,
                                            out var unsolvedClueCount);
    if (lastUnsolvedClue == null || unsolvedClueCount > 1) {
      yield break;
    }

    var cellStates = lineState.CellStates;
    var length = cellStates.Count;

    int? firstUnclaimedFilledCellIndex = null;
    int? lastUnclaimedFilledCellIndex = null;

    // Find first and last unclaimed filled cells in row
    for (var i = 0; i < length; ++i) {
      var cellState = cellStates[i];
      var isUnclaimed = cellState.Status == PicrossCellStatus.KNOWN_FILLED &&
                        (lineState.IsColumn
                            ? cellState.ColumnClue == null
                            : cellState.RowClue == null);

      if (!isUnclaimed) {
        continue;
      }

      firstUnclaimedFilledCellIndex ??= i;
      lastUnclaimedFilledCellIndex = i;
    }

    // If there are no unclaimed cells, there's nothing we can do
    if (firstUnclaimedFilledCellIndex == null ||
        lastUnclaimedFilledCellIndex == null) {
      yield break;
    }

    // Fill in any cells between the first and last filled unclaimed cell
    for (var i = firstUnclaimedFilledCellIndex.Value + 1;
         i < lastUnclaimedFilledCellIndex.Value;
         ++i) {
      if (cellStates[i].Status == PicrossCellStatus.UNKNOWN) {
        yield return new PicrossCellMove1d(
            PicrossCellMoveType.MARK_FILLED,
            PicrossCellMoveSource.WITHIN_LAST_CLUE,
            i);
      }
    }

    // Get the total accounted for length of the clue
    var clueLength = lastUnsolvedClue.Length;
    var filledLength = lastUnclaimedFilledCellIndex.Value -
                       firstUnclaimedFilledCellIndex.Value +
                       1;

    // If it's already the full length, we're done!
    if (clueLength == filledLength) {
      yield break;
    }

    // Otherwise, we need to fill in cells that are too far away
    var remainingClueLength = clueLength - filledLength;

    var remainingClueLengthBefore = remainingClueLength;
    var knownCellsAfter = 0;
    for (var i = firstUnclaimedFilledCellIndex.Value - 1; i >= 0; --i) {
      var cellStatus = cellStates[i].Status;
      if (cellStatus == PicrossCellStatus.UNKNOWN) {
        if (remainingClueLengthBefore-- <= 0) {
          yield return new PicrossCellMove1d(
              PicrossCellMoveType.MARK_EMPTY,
              PicrossCellMoveSource.TOO_FAR_FROM_KNOWN,
              i);
        }
      } else {
        if (remainingClueLengthBefore > 0) {
          knownCellsAfter = remainingClueLengthBefore;
        }

        remainingClueLengthBefore = 0;
      }
    }

    if (remainingClueLengthBefore > 0) {
      knownCellsAfter = remainingClueLengthBefore;
    }


    var remainingClueLengthAfter = remainingClueLength;
    var knownCellsBefore = 0;
    for (var i = lastUnclaimedFilledCellIndex.Value + 1; i < length; ++i) {
      var cellStatus = cellStates[i].Status;
      if (cellStatus == PicrossCellStatus.UNKNOWN) {
        if (remainingClueLengthAfter-- <= 0) {
          yield return new PicrossCellMove1d(
              PicrossCellMoveType.MARK_EMPTY,
              PicrossCellMoveSource.TOO_FAR_FROM_KNOWN,
              i);
        }
      } else {
        if (remainingClueLengthAfter > 0) {
          knownCellsBefore = remainingClueLengthAfter;
        }

        remainingClueLengthAfter = 0;
      }
    }

    if (remainingClueLengthAfter > 0) {
      knownCellsBefore = remainingClueLengthAfter;
    }

    // If not enough gap before/after, we can fill cells in on the other side
    for (var ii = 0; ii < knownCellsBefore; ++ii) {
      var i = firstUnclaimedFilledCellIndex.Value - 1 - ii;
      if (cellStates[i].Status == PicrossCellStatus.UNKNOWN) {
        yield return new PicrossCellMove1d(
            PicrossCellMoveType.MARK_FILLED,
            PicrossCellMoveSource.NOWHERE_ELSE_TO_GO,
            i);
      }
    }

    for (var ii = 0; ii < knownCellsAfter; ++ii) {
      var i = lastUnclaimedFilledCellIndex.Value + 1 + ii;
      var cellStatus = cellStates[i].Status;
      if (cellStatus == PicrossCellStatus.UNKNOWN) {
        yield return new PicrossCellMove1d(
            PicrossCellMoveType.MARK_FILLED,
            PicrossCellMoveSource.NOWHERE_ELSE_TO_GO,
            i);
      }
    }
  }
}