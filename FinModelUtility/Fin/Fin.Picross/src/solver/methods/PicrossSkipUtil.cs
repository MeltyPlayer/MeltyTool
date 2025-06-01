using fin.math;

namespace fin.picross.solver.methods;

public static class PicrossSkipUtil {
  public static void SkipSolvedClues(
      IPicrossLineState lineState,
      int increment,
      ref int clueStart,
      ref int iStart) {
    var cluesStates = lineState.ClueStates;

    // Move past all starting solved clues in line
    while (cluesStates[clueStart].Solved) {
      var clueState = cluesStates[clueStart];
      clueStart += increment;

      if (increment == 1) {
        iStart = clueState.StartIndex.Value + clueState.Length;
      } else {
        iStart = clueState.StartIndex.Value + increment;
      }
    }
  }

  public static int SkipEmpty(
      IPicrossLineState lineState,
      int increment,
      ref int iStart) {
    var count = 0;

    var cellStates = lineState.CellStates;
    var length = cellStates.Count;
    while (iStart.IsInRange(0, length - 1) &&
           cellStates[iStart].Status == PicrossCellStatus.KNOWN_EMPTY) {
      iStart += increment;
      ++count;
    }

    return count;
  }

  public static int SkipNonEmpty(
      IPicrossLineState lineState,
      int increment,
      ref int iStart) {
    var count = 0;

    var cellStates = lineState.CellStates;
    var length = cellStates.Count;
    while (iStart.IsInRange(0, length - 1) &&
           cellStates[iStart].Status != PicrossCellStatus.KNOWN_EMPTY) {
      iStart += increment;
      ++count;
    }

    return count;
  }
}