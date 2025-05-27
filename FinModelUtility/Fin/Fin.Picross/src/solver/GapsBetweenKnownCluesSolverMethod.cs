using fin.picross.moves;

namespace fin.picross.solver;

public class GapsBetweenKnownCluesSolverMethod : IPicrossSolverMethod {
  public IEnumerable<IPicrossMove1d>
      TryToFindMoves(IPicrossLineState lineState) {
    var clueStates = lineState.ClueStates;
    var cellStates = lineState.CellStates;
    var length = cellStates.Count;

    var firstClueState = clueStates[0];
    if (firstClueState.Solved) {
      for (var i = 0; i < firstClueState.StartIndex; ++i) {
        if (cellStates[i].Status == PicrossCellStatus.UNKNOWN) {
          yield return new PicrossCellMove1d(
              PicrossCellMoveType.MARK_EMPTY,
              PicrossCellMoveSource.EMPTY_BETWEEN_CLUES,
              i);
        }
      }
    }

    for (var clueI = 0; clueI < clueStates.Count - 1; ++clueI) {
      var currentClueState = clueStates[clueI];
      var nextClueState = clueStates[clueI + 1];

      if (currentClueState.Solved && nextClueState.Solved) {
        for (var i = currentClueState.StartIndex.Value +
                     currentClueState.Length;
             i < nextClueState.StartIndex;
             ++i) {
          if (cellStates[i].Status == PicrossCellStatus.UNKNOWN) {
            yield return new PicrossCellMove1d(
                PicrossCellMoveType.MARK_EMPTY,
                PicrossCellMoveSource.EMPTY_BETWEEN_CLUES,
                i);
          }
        }
      }
    }

    var lastClueState = clueStates[^1];
    if (lastClueState.Solved) {
      for (var i = lastClueState.StartIndex.Value + lastClueState.Length;
           i < length;
           ++i) {
        if (cellStates[i].Status == PicrossCellStatus.UNKNOWN) {
          yield return new PicrossCellMove1d(
              PicrossCellMoveType.MARK_EMPTY,
              PicrossCellMoveSource.EMPTY_BETWEEN_CLUES,
              i);
        }
      }
    }
  }
}