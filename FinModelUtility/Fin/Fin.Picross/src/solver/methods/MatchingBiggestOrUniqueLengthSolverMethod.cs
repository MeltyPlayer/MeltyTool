using fin.picross.moves;
using fin.util.enumerables;

namespace fin.picross.solver.methods;

public class MatchingBiggestOrUniqueLengthSolverMethod : IPicrossSolverMethod {
  public IEnumerable<IPicrossMove1d> TryToFindMoves(
      IPicrossLineState lineState) {
    var clueStates = lineState.ClueStates;
    var cellStates = lineState.CellStates;
    var length = cellStates.Count;

    if (clueStates.All(c => c.Solved)) {
      yield break;
    }

    var biggestUnusedLength
        = clueStates.Where(c => !c.Solved).Max(c => c.Length);

    int startIndex = -1;
    var inClue = false;

    for (var i = 0; i < length; ++i) {
      var cell = cellStates[i].Status == PicrossCellStatus.KNOWN_FILLED;

      if (cell && !inClue) {
        startIndex = i;
      }

      if (!cell && inClue) {
        var clueLength = i - startIndex;

        var beforeI = startIndex - 1;
        var afterI = i;

        foreach (var move in CheckIfMatchingClue_(
                     clueStates,
                     biggestUnusedLength,
                     startIndex,
                     clueLength,
                     beforeI < 0 ||
                     cellStates[beforeI].Status ==
                     PicrossCellStatus.KNOWN_EMPTY,
                     cellStates[afterI].Status ==
                     PicrossCellStatus.KNOWN_EMPTY)) {
          yield return move;
        }
      }

      inClue = cell;
    }

    if (inClue) {
      var clueLength = length - startIndex;
      var beforeI = startIndex - 1;
      foreach (var move in CheckIfMatchingClue_(
                   clueStates,
                   biggestUnusedLength,
                   startIndex,
                   clueLength,
                   cellStates[beforeI].Status == PicrossCellStatus.KNOWN_EMPTY,
                   true)) {
        yield return move;
      }
    }
  }

  private static IEnumerable<IPicrossMove1d> CheckIfMatchingClue_(
      IReadOnlyList<IReadOnlyPicrossClueState> clueStates,
      int biggestUnusedClueLength,
      int startI,
      int clueLength,
      bool knownEmptyBefore,
      bool knownEmptyAfter) {
    if (clueStates.Any(c => c.StartIndex == startI)) {
      yield break;
    }

    var firstClueWithExactLength
        = clueStates.Where(c => !c.Solved)
                    .FirstOrDefaultAndCount(c => c.Length == clueLength,
                                            out var matchingClueCount);
    if (firstClueWithExactLength == null) {
      yield break;
    }

    var isBiggest = clueLength == biggestUnusedClueLength;

    if (matchingClueCount == 1 &&
        (isBiggest || (knownEmptyBefore && knownEmptyAfter))) {
      if (clueStates is [{ Length: 2 }, { Length: 2 }, { Length: 1 }] &&
          startI == 2) {
        ;
      }

      yield return new PicrossClueMove(
          PicrossClueMoveSource.ONLY_MATCHING_CLUE,
          firstClueWithExactLength.Clue,
          startI);
    }

    if (isBiggest) {
      if (!knownEmptyBefore) {
        yield return new PicrossCellMove1d(
            PicrossCellMoveType.MARK_EMPTY,
            PicrossCellMoveSource.EMPTY_AROUND_KNOWN_CLUE,
            startI - 1);
      }

      if (!knownEmptyAfter) {
        yield return new PicrossCellMove1d(
            PicrossCellMoveType.MARK_EMPTY,
            PicrossCellMoveSource.EMPTY_AROUND_KNOWN_CLUE,
            startI + clueLength);
      }
    }
  }
}