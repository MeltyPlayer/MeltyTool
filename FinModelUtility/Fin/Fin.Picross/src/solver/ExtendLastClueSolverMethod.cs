using fin.math;
using fin.picross.moves;
using fin.util.enumerables;

namespace fin.picross.solver;

public class ExtendLastClueSolverMethod : BBidirectionalSolverMethod {
  public override IEnumerable<PicrossCellMove1d> TryToFindMoves(
      IPicrossLineState lineState,
      int iStart,
      int iEnd,
      int clueStart,
      int clueEnd,
      int increment) {
    var clueStates = lineState.ClueStates;
    var cellStates = lineState.CellStates;
    var length = cellStates.Count;

    if (clueStates.Count(c => !c.Used) != 1) {
      yield break;
    }

    var unsolvedClueI = clueStates.IndexOf(c => !c.Used);
    if (unsolvedClueI != clueEnd - increment) {
      yield break;
    }

    var unsolvedClue = clueStates[unsolvedClueI];
    var previousEmptyI = iStart;

    var inClue = false;
    var clueI = clueStart - increment;
    for (var i = iStart; i != iEnd; i += increment) {
      var cell = cellStates[i].Status == PicrossCellStatus.KNOWN_FILLED;

      var newlyInClue = cell && !inClue;
      if (newlyInClue) {
        clueI += increment;
      }

      var newlyOutOfClue = !cell && inClue;
      if (newlyOutOfClue ||
          cellStates[i].Status == PicrossCellStatus.KNOWN_EMPTY) {
        previousEmptyI = i + increment;
      }

      inClue = cell;

      // TODO: Not working?
      if (clueI + increment != unsolvedClueI &&
          cellStates[i].Status == PicrossCellStatus.UNKNOWN) {
        yield return new PicrossCellMove1d(
            PicrossCellMoveType.MARK_EMPTY,
            PicrossCellMoveSource.EMPTY_BETWEEN_CLUES,
            i);
      }

      if (clueI == unsolvedClueI && newlyInClue) {
        var distanceToPreviousEmpty = increment * (i - previousEmptyI);
        var remainingLength = unsolvedClue.Length - distanceToPreviousEmpty;
        for (var clueCellI = 0; clueCellI < remainingLength; ++clueCellI) {
          var ii = i + increment * clueCellI;
          if (ii.IsInRange(0, length - 1) &&
              cellStates[ii].Status == PicrossCellStatus.UNKNOWN) {
            yield return new PicrossCellMove1d(
                PicrossCellMoveType.MARK_FILLED,
                PicrossCellMoveSource.NOWHERE_ELSE_TO_GO,
                ii);
          }
        }

        yield break;
      }
    }
  }
}