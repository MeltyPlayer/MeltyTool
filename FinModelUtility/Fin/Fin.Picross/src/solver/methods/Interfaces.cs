using fin.picross.moves;

namespace fin.picross.solver.methods;

public interface IPicrossSolverMethod {
  IEnumerable<IPicrossMove1d> TryToFindMoves(IPicrossLineState lineState);
}