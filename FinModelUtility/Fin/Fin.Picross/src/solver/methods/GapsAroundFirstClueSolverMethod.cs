using fin.picross.moves;

namespace fin.picross.solver.methods;

public class GapsAroundFirstClueSolverMethod : BBidirectionalSolverMethod {
  public override IEnumerable<PicrossCellMove1d> TryToFindMoves(
      IPicrossBoardState _,
      IPicrossLineState lineState,
      int iStart,
      int iEnd,
      int clueStart,
      int clueEnd,
      int increment) {
    var clues = lineState.ClueStates;
    var cellStates = lineState.CellStates;

    if (clues.Count != 2 || clues.Any(c => c.Solved)) {
      yield break;
    }

    var groupCount = 0;
    var groupLength = 0;
    int? firstGroupIndex = null;
    int firstGroupLength = 0;
    int? lastGroupIndex = null;

    var inClue = false;
    for (var i = iStart; i != iEnd; i += increment) {
      var cell = cellStates[i].Status == PicrossCellStatus.KNOWN_FILLED;

      var newlyInClue = cell && !inClue;
      if (newlyInClue) {
        groupLength = 0;
        if (groupCount++ == 0) {
          firstGroupIndex = i;
        }
      }

      inClue = cell;

      if (inClue) {
        ++groupLength;
        if (groupCount == 1) {
          firstGroupLength = groupLength;
        }

        lastGroupIndex = i;
      }
    }

    if (firstGroupLength != clues[clueStart].Length) {
      yield break;
    }

    if (groupCount < 2) {
      yield break;
    }

    var totalLength
        = Math.Abs(lastGroupIndex.Value - firstGroupIndex.Value) + 1;
    if (totalLength <= clues[clueEnd - increment].Length) {
      yield break;
    }

    for (var i = iStart; i != firstGroupIndex.Value; i += increment) {
      if (cellStates[i].Status == PicrossCellStatus.UNKNOWN) {
        yield return new PicrossCellMove1d(
            PicrossCellMoveType.MARK_EMPTY,
            PicrossCellMoveSource.EMPTY_UP_TO_FIRST_CLUE,
            i);
      }
    }

    var afterFirstClueI = firstGroupIndex.Value + increment * firstGroupLength;
    if (cellStates[afterFirstClueI].Status == PicrossCellStatus.UNKNOWN) {
      yield return new PicrossCellMove1d(
          PicrossCellMoveType.MARK_EMPTY,
          PicrossCellMoveSource.EMPTY_AROUND_KNOWN_CLUE,
          afterFirstClueI);
    }
  }
}