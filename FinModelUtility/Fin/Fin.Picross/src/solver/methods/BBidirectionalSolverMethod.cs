using fin.picross.moves;

namespace fin.picross.solver.methods;

public abstract class BBidirectionalSolverMethod : IPicrossSolverMethod {
  public abstract IEnumerable<IPicrossMove1d> TryToFindMoves(
      IPicrossLineState lineState,
      int iStart,
      int iEnd,
      int clueStart,
      int clueEnd,
      int increment);

  public IEnumerable<IPicrossMove1d> TryToFindMoves(
      IPicrossLineState lineState) {
    var clues = lineState.ClueStates;
    var cellStates = lineState.CellStates;

    var forwardMoves
        = this.TryToFindMoves(lineState,
                              0,
                              cellStates.Count,
                              0,
                              clues.Count,
                              1);
    var backwardMoves
        = this.TryToFindMoves(lineState,
                              cellStates.Count - 1,
                              -1,
                              clues.Count - 1,
                              -1,
                              -1);

    foreach (var move in forwardMoves.Concat(backwardMoves)) {
      yield return move;
    }
  }
}