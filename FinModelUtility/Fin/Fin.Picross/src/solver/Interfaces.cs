using fin.picross.moves;

namespace fin.picross.solver;

public interface IPicrossSolverMethod {
  IEnumerable<IPicrossMove1d> TryToFindMoves(IPicrossLineState lineState);
}