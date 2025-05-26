using fin.picross.moves;

namespace fin.picross.solver;

public class ExtendFirstClueSolverMethod : BBidirectionalSolverMethod {
  public override IEnumerable<PicrossCellMove1d> TryToFindMoves(
      IPicrossLineState lineState,
      int iStart,
      int iEnd,
      int clueStart,
      int clueEnd,
      int increment) {
    var clues = lineState.Clues;
    var cellStates = lineState.CellStates;

    var clue = clues[clueStart];
    var clueLength = clue.Length;

    var endOfClue = iStart + increment * clueLength;
    if (endOfClue == iEnd ||
        cellStates[endOfClue].Status != PicrossCellStatus.KNOWN_EMPTY) {
      yield break;
    }

    var anyFilled = false;
    for (var i = iStart; i != endOfClue; i += increment) {
      if (cellStates[i].Status == PicrossCellStatus.KNOWN_FILLED) {
        anyFilled = true;
        break;
      }
    }

    if (!anyFilled) {
      yield break;
    }

    for (var i = iStart; i != endOfClue; i += increment) {
      if (cellStates[i].Status == PicrossCellStatus.UNKNOWN) {
        yield return new PicrossCellMove1d(
            PicrossCellMoveType.MARK_FILLED,
            PicrossCellMoveSource.NOWHERE_ELSE_TO_GO,
            i);
        break;
      }
    }
  }
}