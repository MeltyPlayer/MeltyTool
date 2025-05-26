using fin.picross.moves;

namespace fin.picross.solver;

public class AlreadySolvedPicrossSolverMethod : IPicrossSolverMethod {
  public IEnumerable<IPicrossMove1d> TryToFindMoves(
      IPicrossLineState lineState) {
    var clueStates = lineState.ClueStates;
    var cellStates = lineState.CellStates;
    var length = cellStates.Count;

    var alreadySolved = clueStates.All(c => c.Used);
    if (!alreadySolved) {
      var expectedCount = clueStates.Sum(c => c.Length);
      var actualCount
          = cellStates.Sum(c => c.Status == PicrossCellStatus.KNOWN_FILLED
                               ? 1
                               : 0);
      alreadySolved = expectedCount == actualCount;
    }

    if (alreadySolved) {
      if (clueStates is [{ Length: 0 }]) {
        yield return new PicrossClueMove(
            PicrossClueMoveSource.ALL_CLUES_SOLVED,
            clueStates[0].Clue,
            0
        );
      } else {
        var cellI = 0;
        foreach (var clueState in clueStates) {
          while (cellStates[cellI].Status != PicrossCellStatus.KNOWN_FILLED) {
            ++cellI;
          }

          if (!clueState.Used) {
            yield return new PicrossClueMove(
                PicrossClueMoveSource.ALL_CLUES_SOLVED,
                clueState.Clue,
                cellI
            );
          }

          cellI += clueState.Length;
        }
      }

      for (var i = 0; i < length; ++i) {
        if (cellStates[i].Status == PicrossCellStatus.UNKNOWN) {
          yield return new PicrossCellMove1d(
              PicrossCellMoveType.MARK_EMPTY,
              PicrossCellMoveSource.ALL_CLUES_SOLVED,
              i);
        }
      }
    }
  }
}