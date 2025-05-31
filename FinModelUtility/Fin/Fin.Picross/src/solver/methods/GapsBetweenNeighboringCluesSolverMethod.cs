using fin.picross.moves;

namespace fin.picross.solver.methods;

public class GapsBetweenNeighboringCluesSolverMethod : IPicrossSolverMethod {
  public IEnumerable<IPicrossMove1d> TryToFindMoves(
      IPicrossBoardState _,
      IPicrossLineState lineState) {
    var clues = lineState.ClueStates;
    var cellStates = lineState.CellStates;
    var length = cellStates.Count;

    if (clues.All(c => c.Solved)) {
      yield break;
    }

    var biggestLength = clues.Where(c => !c.Solved).Max(c => c.Length);

    var cell0 = cellStates[0].Status;
    var cell1 = cellStates[1].Status;

    for (var i = 2; i < length; ++i) {
      var cell2 = cellStates[i].Status;

      if (cell0 == PicrossCellStatus.KNOWN_FILLED &&
          cell1 == PicrossCellStatus.UNKNOWN &&
          cell2 == PicrossCellStatus.KNOWN_FILLED) {
        var lengthLhs = 0;
        for (var ii = i - 2; ii >= 0; --ii) {
          if (cellStates[ii].Status == PicrossCellStatus.KNOWN_FILLED) {
            ++lengthLhs;
          } else {
            break;
          }
        }

        var lengthRhs = 0;
        for (var ii = i; ii < length; ++ii) {
          if (cellStates[ii].Status == PicrossCellStatus.KNOWN_FILLED) {
            ++lengthRhs;
          } else {
            break;
          }
        }

        if (lengthLhs + 1 + lengthRhs > biggestLength) {
          yield return new PicrossCellMove1d(
              PicrossCellMoveType.MARK_EMPTY,
              PicrossCellMoveSource.EMPTY_BETWEEN_CLUES,
              i - 1);
        }
      }

      cell0 = cell1;
      cell1 = cell2;
    }
  }
}