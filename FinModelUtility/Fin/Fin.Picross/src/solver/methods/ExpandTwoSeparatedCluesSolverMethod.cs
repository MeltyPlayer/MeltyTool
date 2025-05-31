using fin.picross.moves;

namespace fin.picross.solver.methods;

public class ExpandTwoSeparatedCluesSolverMethod : IPicrossSolverMethod {
  public IEnumerable<IPicrossMove1d>
      TryToFindMoves(IPicrossLineState lineState) {
    var clueStates = lineState.ClueStates;
    if (clueStates is not [
            { Solved: false } firstClue,
            { Solved: false } secondClue
        ]) {
      yield break;
    }

    var cellStates = lineState.CellStates;
    var length = cellStates.Count;

    // Find first and last unclaimed filled cells in row
    int? lastCellOfFirstClue = null;
    int? firstEmptyCellOfGap = null;
    int? lastEmptyCellOfGap = null;
    int? firstCellOfSecondClue = null;
    for (var i = 0; i < length; ++i) {
      var cellStatus = cellStates[i].Status;

      if (lastEmptyCellOfGap == null &&
          cellStatus == PicrossCellStatus.KNOWN_FILLED) {
        lastCellOfFirstClue = i;
      }

      if (lastCellOfFirstClue != null &&
          cellStatus == PicrossCellStatus.KNOWN_EMPTY) {
        firstEmptyCellOfGap ??= i;
        lastEmptyCellOfGap = i;
      }

      if (lastEmptyCellOfGap != null &&
          cellStatus == PicrossCellStatus.KNOWN_FILLED) {
        firstCellOfSecondClue = i;
        break;
      }
    }

    // If there are not two separate clues, there's nothing we can do.
    if (lastCellOfFirstClue == null || firstCellOfSecondClue == null) {
      yield break;
    }

    // Mark cells within the gap as empty
    for (var i = firstEmptyCellOfGap.Value + 1;
         i < lastEmptyCellOfGap.Value;
         ++i) {
      if (cellStates[i].Status == PicrossCellStatus.UNKNOWN) {
        yield return new PicrossCellMove1d(
            PicrossCellMoveType.MARK_EMPTY,
            PicrossCellMoveSource.EMPTY_BETWEEN_CLUES,
            i);
      }
    }

    foreach (var move in PicrossMultiMoveUtil.AlignGapToClue(
                 lineState,
                 firstClue.Length,
                 0,
                 firstEmptyCellOfGap.Value)) {
      yield return move;
    }

    foreach (var move in PicrossMultiMoveUtil.AlignGapToClue(
                 lineState,
                 secondClue.Length,
                 lastEmptyCellOfGap.Value + 1,
                 length)) {
      yield return move;
    }
  }
}