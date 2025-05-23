namespace fin.picross.solver;

public class GapsBetweenNeighboringShortCluesSolverMethod
    : IPicrossSolverMethod {
  public IEnumerable<PicrossMove1d> TryToFindMoves(
      IPicrossLineState lineState) {
    var clues = lineState.Clues;
    var cellStates = lineState.CellStates;
    var length = cellStates.Count;

    var biggestLength = clues.Max(c => c.Length);
    if (biggestLength >= 3) {
      yield break;
    }

    var cell0 = cellStates[0].Status;
    var cell1 = cellStates[1].Status;

    for (var i = 2; i < length; ++i) {
      var cell2 = cellStates[i].Status;

      if (cell0 == PicrossCellStatus.KNOWN_FILLED &&
          cell1 == PicrossCellStatus.UNKNOWN &&
          cell2 == PicrossCellStatus.KNOWN_FILLED) {
        yield return new PicrossMove1d(
            PicrossMoveType.MARK_EMPTY,
            PicrossMoveSource.EMPTY_BETWEEN_CLUES,
            i - 1);
      }

      cell0 = cell1;
      cell1 = cell2;
    }
  }
}