namespace fin.picross.solver;

public class AlreadySolvedPicrossSolverMethod : IPicrossSolverMethod {
  public IEnumerable<PicrossMove1d> TryToFindMoves(
      IPicrossLineState lineState) {
    var clues = lineState.Clues;
    var cellStates = lineState.CellStates;
    var length = cellStates.Count;

    var alreadySolved = clues.All(c => c.Used);
    if (!alreadySolved) {
      var expectedCount = clues.Sum(c => c.Length);
      var actualCount
          = cellStates.Sum(c => c.Status == PicrossCellStatus.KNOWN_FILLED
                               ? 1
                               : 0);
      alreadySolved = expectedCount == actualCount;
    }

    if (alreadySolved) {
      foreach (var clue in clues) {
        clue.Used = true;
      }

      for (var i = 0; i < length; ++i) {
        if (cellStates[i].Status == PicrossCellStatus.UNKNOWN) {
          yield return new PicrossMove1d(
              PicrossMoveType.MARK_EMPTY,
              PicrossMoveSource.ALL_CLUES_SOLVED,
              i);
        }
      }
    }
  }
}