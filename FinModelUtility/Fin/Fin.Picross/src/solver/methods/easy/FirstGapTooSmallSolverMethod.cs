using fin.picross.moves;

namespace fin.picross.solver.methods.easy;

public class FirstGapTooSmallSolverMethod : BBidirectionalSolverMethod {
  public override IEnumerable<IPicrossMove1d> TryToFindMoves(
      IPicrossBoardState boardState,
      IPicrossLineState lineState,
      int iStart,
      int iEnd,
      int clueStart,
      int clueEnd,
      int increment) {
    if (lineState.IsSolved) {
      yield break;
    }
    
    var clueStates = lineState.ClueStates;
    var cellStates = lineState.CellStates;

    PicrossSkipUtil.SkipSolvedClues(
        lineState,
        increment,
        ref clueStart,
        ref iStart);
    var firstUnsolvedClue = clueStates[clueStart];

    PicrossSkipUtil.SkipEmpty(lineState, increment, ref iStart);

    var iGapStart = iStart;
    var gapSize
        = PicrossSkipUtil.SkipNonEmpty(lineState, increment, ref iStart);
    var iGapEnd = iStart;

    if (gapSize >= firstUnsolvedClue.Length) {
      yield break;
    }

    for (var i = iGapStart; i != iGapEnd; i += increment) {
      if (cellStates[i].Status == PicrossCellStatus.UNKNOWN) {
        yield return new PicrossCellMove1d(
            PicrossCellMoveType.MARK_EMPTY,
            PicrossCellMoveSource.NO_CLUES_FIT,
            i);
      }
    }
  }
}