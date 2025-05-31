using fin.picross.moves;

namespace fin.picross.solver.methods.easy;

public class AlreadySolvedUpToPicrossSolverMethod : BBidirectionalSolverMethod {
  public override IEnumerable<IPicrossMove1d> TryToFindMoves(
      IPicrossBoardState _,
      IPicrossLineState lineState,
      int iStart,
      int iEnd,
      int clueStart,
      int clueEnd,
      int increment) {
    if (lineState.IsSolved) {
      yield break;
    }

    var clues = lineState.ClueStates;
    var cellStates = lineState.CellStates;

    PicrossSkipUtil.SkipSolvedClues(
        lineState,
        increment,
        ref clueStart,
        ref iStart);

    PicrossSkipUtil.SkipEmpty(lineState, increment, ref iStart);

    var cell0 = cellStates[iStart].Status;
    // We allow one empty, just in case.
    if (cell0 == PicrossCellStatus.UNKNOWN) {
      iStart += increment;
    }

    var nextUnsolvedClue = clues[clueStart];

    var clueLength = 0;
    for (var i = iStart; i != iEnd; i += increment) {
      if (cellStates[i].Status == PicrossCellStatus.KNOWN_FILLED) {
        ++clueLength;
      } else {
        break;
      }
    }

    if (clueLength == nextUnsolvedClue.Length) {
      var absoluteStartI
          = BidirectionalUtil.GetAbsoluteStartI(iStart, clueLength, increment);

      yield return new PicrossClueMove(
          PicrossClueMoveSource.FIRST_CLUE_IN_LINE,
          nextUnsolvedClue.Clue,
          absoluteStartI);
    }
  }
}