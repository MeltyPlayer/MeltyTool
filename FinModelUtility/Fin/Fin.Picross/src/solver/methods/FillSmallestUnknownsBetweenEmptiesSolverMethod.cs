using fin.picross.moves;

namespace fin.picross.solver.methods;

public class FillSmallestUnknownsBetweenEmptiesSolverMethod
    : IPicrossSolverMethod {
  public IEnumerable<IPicrossMove1d> TryToFindMoves(
      IPicrossBoardState _,
      IPicrossLineState lineState) {
    var clues = lineState.ClueStates;
    var cellStates = lineState.CellStates;
    var length = cellStates.Count;

    var smallestClueLength = clues.Min(c => c.Length);
    if (smallestClueLength <= 1) {
      yield break;
    }

    int? firstUnknownIndex = null;

    var inFilled = false;
    var inUnknown = false;

    for (var i = 0; i < length; ++i) {
      var cellStatus = cellStates[i].Status;

      var isUnknown = cellStatus == PicrossCellStatus.UNKNOWN;
      if (isUnknown && !inUnknown && !inFilled) {
        firstUnknownIndex = i;
      }

      var isEmpty = cellStatus == PicrossCellStatus.KNOWN_EMPTY;
      if (isEmpty && inUnknown) {
        if (firstUnknownIndex != null) {
          var unknownLength = i - firstUnknownIndex.Value;
          if (unknownLength < smallestClueLength) {
            for (var ii = firstUnknownIndex.Value; ii < i; ++ii) {
              if (cellStates[ii].Status == PicrossCellStatus.UNKNOWN) {
                yield return new PicrossCellMove1d(
                    PicrossCellMoveType.MARK_EMPTY,
                    PicrossCellMoveSource.NO_CLUES_FIT,
                    ii);
              }
            }
          }
        }
      }

      inFilled = cellStatus == PicrossCellStatus.KNOWN_FILLED;
      inUnknown = isUnknown;
      if (!inUnknown) {
        firstUnknownIndex = null;
      }
    }
  }
}