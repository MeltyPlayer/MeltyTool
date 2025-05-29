using fin.picross.moves;

namespace fin.picross.solver;

public class SingleNextToEdgeSolverMethod : BBidirectionalSolverMethod {
  public override IEnumerable<IPicrossMove1d> TryToFindMoves(
      IPicrossLineState lineState,
      int iStart,
      int iEnd,
      int clueStart,
      int clueEnd,
      int increment) {
    var clues = lineState.ClueStates;
    var cellStates = lineState.CellStates;

    if (clues.All(c => c.Solved)) {
      yield break;
    }

    PicrossSkipUtil.SkipSolvedClues(
        lineState,
        increment,
        ref clueStart,
        ref iStart);

    var unsolvedClue = clues[clueStart];
    if (unsolvedClue.Length != 1) {
      yield break;
    }

    var remainingLength = increment == 1 ? iEnd - iStart : iStart - iEnd;
    if (remainingLength < 3) {
      yield break;
    }

    var i0 = iStart;
    var i1 = i0 + increment;
    var i2 = i1 + increment;
    var cell0 = cellStates[i0].Status;
    var cell1 = cellStates[i1].Status;
    var cell2 = cellStates[i2].Status;

    // In _M_ case, we can fill cells as XMX
    if (cell0 != PicrossCellStatus.KNOWN_FILLED &&
        cell1 == PicrossCellStatus.KNOWN_FILLED &&
        cell2 != PicrossCellStatus.KNOWN_FILLED) {
      if (cell0 == PicrossCellStatus.UNKNOWN) {
        yield return new PicrossCellMove1d(
            PicrossCellMoveType.MARK_EMPTY,
            PicrossCellMoveSource.EMPTY_AROUND_KNOWN_CLUE,
            i0);
      }

      yield return new PicrossClueMove(
          PicrossClueMoveSource.FIRST_CLUE_IN_LINE,
          unsolvedClue.Clue,
          i1);

      if (cell2 == PicrossCellStatus.UNKNOWN) {
        yield return new PicrossCellMove1d(
            PicrossCellMoveType.MARK_EMPTY,
            PicrossCellMoveSource.EMPTY_AROUND_KNOWN_CLUE,
            i2);
      }

      yield break;
    }

    if (remainingLength < 4) {
      yield break;
    }

    var i3 = i2 + increment;
    var cell3 = cellStates[i3].Status;

    // In __M_ case, we can fill cells as _XM_
    if (cell0 != PicrossCellStatus.KNOWN_FILLED &&
        cell1 != PicrossCellStatus.KNOWN_FILLED &&
        cell2 == PicrossCellStatus.KNOWN_FILLED &&
        cell3 != PicrossCellStatus.KNOWN_FILLED) {
      if (cell1 == PicrossCellStatus.UNKNOWN) {
        yield return new PicrossCellMove1d(
            PicrossCellMoveType.MARK_EMPTY,
            PicrossCellMoveSource.EMPTY_AROUND_KNOWN_CLUE,
            i1);
      }
    }
  }
}