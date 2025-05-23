namespace fin.picross.solver;

public class GapsAroundBiggestSolverMethod : IPicrossSolverMethod {
  public IEnumerable<PicrossMove1d> TryToFindMoves(
      IPicrossLineState lineState) {
    var clues = lineState.Clues;
    var cellStates = lineState.CellStates;
    var length = cellStates.Count;

    var biggestLength = clues.Max(c => c.Length);

    int startIndex = -1;
    var inClue = false;

    for (var i = 0; i < length; ++i) {
      var cell = cellStates[i].Status == PicrossCellStatus.KNOWN_FILLED;

      if (cell && !inClue) {
        startIndex = i;
      }

      if (!cell && inClue) {
        var clueLength = i - startIndex;
        if (clueLength == biggestLength) {
          var beforeI = startIndex - 1;
          if (beforeI >= 0 &&
              cellStates[beforeI].Status == PicrossCellStatus.UNKNOWN) {
            yield return new PicrossMove1d(
                PicrossMoveType.MARK_EMPTY,
                PicrossMoveSource.EMPTY_AROUND_KNOWN_CLUE,
                beforeI);
          }

          var afterI = i;
          if (cellStates[afterI].Status == PicrossCellStatus.UNKNOWN) {
            yield return new PicrossMove1d(
                PicrossMoveType.MARK_EMPTY,
                PicrossMoveSource.EMPTY_AROUND_KNOWN_CLUE,
                afterI);
          }
        }
      }

      inClue = cell;
    }
  }
}