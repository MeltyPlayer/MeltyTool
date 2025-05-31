using fin.picross.moves;
using fin.util.enumerables;

namespace fin.picross.solver.methods;

public class ExpandFinalUnsolvedClueSolverMethod : IPicrossSolverMethod {
  public IEnumerable<IPicrossMove1d> TryToFindMoves(
      IPicrossBoardState _,
      IPicrossLineState lineState) {
    // Make sure we only run this solver if there's a single unsolved clue
    var clueStates = lineState.ClueStates;
    var lastUnsolvedClue
        = clueStates.FirstOrDefaultAndCount(c => !c.Solved,
                                            out var unsolvedClueCount);
    if (lastUnsolvedClue == null || unsolvedClueCount > 1) {
      yield break;
    }

    var cellStates = lineState.CellStates;
    var length = cellStates.Count;

    foreach (var move in PicrossMultiMoveUtil.AlignGapToClue(
                 lineState,
                 lastUnsolvedClue.Length,
                 0,
                 length)) {
      yield return move;
    }
  }
}