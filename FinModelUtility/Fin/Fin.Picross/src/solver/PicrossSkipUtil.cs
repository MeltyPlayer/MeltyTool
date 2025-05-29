namespace fin.picross.solver;

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

  public static void SkipEmpty(
      IPicrossLineState lineState,
      int increment,
      ref int iStart) {
    var cellStates = lineState.CellStates;
    while (cellStates[iStart].Status == PicrossCellStatus.KNOWN_EMPTY) {
      iStart += increment;
    }
  }
}