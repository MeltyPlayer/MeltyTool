using fin.data;
using fin.math;
using fin.util.asserts;

using Generator.Equals;

namespace fin.picross;

using PicrossMove1d
    = (PicrossMoveType MoveType, PicrossMoveSource MoveSource, int i);

[Equatable]
public readonly partial record struct PicrossMove(
    PicrossMoveType MoveType,
    PicrossMoveSource MoveSource,
    int X,
    int Y) {
  [IgnoreEquality]
  public PicrossMoveSource MoveSource { get; } = MoveSource;
}

public enum PicrossMoveType {
  MARK_EMPTY,
  MARK_FILLED
}

public enum PicrossMoveSource {
  FREEBIE_EMPTY,
  FREEBIE_FULL_LENGTH,
  FREEBIE_PERFECT_FIT,
  END_DOESNT_FIT,
  NOWHERE_ELSE_TO_GO,
  FORWARD_BACKWARD_OVERLAP,
  TOO_FAR_FROM_KNOWN,
  ALL_CLUES_SOLVED,
  EMPTY_BETWEEN_CLUES,
  EMPTY_AROUND_ONES,
}

public class PicrossSolver {
  public IReadOnlyList<IReadOnlySet<PicrossMove>> Solve(
      IPicrossDefinition picrossDefinition,
      out PicrossBoardState finalBoardState) {
    var clues = new PicrossCluesGenerator().GenerateClues(picrossDefinition);
    var clueColumns = ToMutableClues_(clues.Columns);
    var clueRows = ToMutableClues_(clues.Rows);

    var width = picrossDefinition.Width;
    var height = picrossDefinition.Height;

    var xCellStates = new PicrossCellState[width];
    var yCellStates = new PicrossCellState[height];

    var xClueIndicesForward = new int[width];
    var xClueIndicesBackward = new int[width];
    var yClueIndicesForward = new int[height];
    var yClueIndicesBackward = new int[height];

    var boardState = new PicrossBoardState(picrossDefinition);
    var moveSets = new List<IReadOnlySet<PicrossMove>>();
    while (true) {
      var isFirstPass = moveSets.Count == 0;
      var moveSet = new HashSet<PicrossMove>();

      for (var x = 0; x < width; ++x) {
        var clueColumn = clueColumns[x];
        if (clueColumn.All(c => c.Used)) {
          continue;
        }

        boardState.GetColumn(x, xCellStates);
        foreach (var (moveType, moveSource, y) in CheckClues_(
                     isFirstPass,
                     xCellStates,
                     clueColumn,
                     height,
                     yClueIndicesForward,
                     yClueIndicesBackward)) {
          moveSet.Add(new PicrossMove(moveType, moveSource, x, y));
        }
      }

      for (var y = 0; y < height; ++y) {
        var clueRow = clueRows[y];
        if (clueRow.All(c => c.Used)) {
          continue;
        }

        if (y == 2) {
          ;
        }

        boardState.GetRow(y, yCellStates);
        foreach (var (moveType, moveSource, x) in CheckClues_(
                     isFirstPass,
                     yCellStates,
                     clueRow,
                     width,
                     xClueIndicesForward,
                     xClueIndicesBackward)) {
          moveSet.Add(new PicrossMove(moveType, moveSource, x, y));
        }
      }

      // If no more moves, then nothing else to do... either complete or stuck.
      if (!isFirstPass && moveSet.Count == 0) {
        break;
      }

      // Verifies moves are correct against the existing board.
      foreach (var (moveType, moveSource, x, y) in moveSet) {
        var expected = moveType == PicrossMoveType.MARK_FILLED;
        Asserts.Equal(expected,
                      picrossDefinition[x, y],
                      $"Incorrect move of source {moveSource}.");
      }

      boardState.ApplyMoves(moveSet);
      moveSets.Add(moveSet);
    }

    finalBoardState = boardState;
    return moveSets;
  }

  private static IEnumerable<PicrossMove1d> CheckClues_(
      bool isFirstPass,
      IReadOnlyList<PicrossCellState> cellStates,
      IReadOnlyList<PicrossClue> clues,
      int length,
      int[] clueIndicesForward,
      int[] clueIndicesBackward) {
    if (isFirstPass) {
      // Freebie (0 or full length)
      if (clues.Count == 1) {
        var clue = clues[0];
        var clueLength = clue.Length;
        var isEmpty = clueLength == 0;
        if (isEmpty || clueLength == length) {
          var moveType = isEmpty
              ? PicrossMoveType.MARK_EMPTY
              : PicrossMoveType.MARK_FILLED;
          var moveSource = isEmpty
              ? PicrossMoveSource.FREEBIE_EMPTY
              : PicrossMoveSource.FREEBIE_FULL_LENGTH;
          clue.Used = true;
          for (var i = 0; i < length; ++i) {
            yield return (moveType, moveSource, i);
          }
        }
      }

      // Freebie (adds up to full width)
      else if ((clues.Sum(c => c.Length) + clues.Count - 1) == length) {
        var lineI = 0;
        for (var c = 0; c < clues.Count; c++) {
          if (c > 0) {
            yield return (PicrossMoveType.MARK_EMPTY,
                          PicrossMoveSource.FREEBIE_PERFECT_FIT,
                          lineI++);
          }

          var clue = clues[c];
          clue.Used = true;
          for (var clueI = 0; clueI < clue.Length; ++clueI) {
            yield return (PicrossMoveType.MARK_FILLED,
                          PicrossMoveSource.FREEBIE_PERFECT_FIT,
                          lineI++);
          }
        }
      }

      yield break;
    }

    // Checks line forward and backward.
    var forwardMoves1d
        = TryToFitCluesIntoGaps_(cellStates, clues, clueIndicesForward, true);
    var backwardMoves1d
        = TryToFitCluesIntoGaps_(cellStates, clues, clueIndicesBackward, false);

    foreach (var picrossMove1d in forwardMoves1d.Concat(backwardMoves1d)) {
      yield return picrossMove1d;
    }

    // Tries to fill in where clues overlap.
    for (var i = 0; i < length; ++i) {
      var clueIndexForward = clueIndicesForward[i];
      var clueIndexBackward = clueIndicesBackward[i];
      if (clueIndexForward != -1 &&
          clueIndexForward == clueIndexBackward &&
          cellStates[i] == PicrossCellState.UNKNOWN) {
        if (clueIndexForward % 2 == 0) {
          yield return (
              PicrossMoveType.MARK_FILLED,
              PicrossMoveSource.FORWARD_BACKWARD_OVERLAP,
              i);
        } else {
          yield return (
              PicrossMoveType.MARK_EMPTY,
              PicrossMoveSource.EMPTY_BETWEEN_CLUES,
              i);
        }
      }
    }

    // Checks if already solved...
    var expectedCount = clues.Sum(c => c.Length);
    var actualCount
        = cellStates.Sum(c => c == PicrossCellState.KNOWN_FILLED ? 1 : 0);
    if (expectedCount == actualCount) {
      foreach (var clue in clues) {
        clue.Used = true;
      }

      for (var i = 0; i < length; ++i) {
        if (cellStates[i] == PicrossCellState.UNKNOWN) {
          yield return (PicrossMoveType.MARK_EMPTY,
                        PicrossMoveSource.ALL_CLUES_SOLVED,
                        i);
        }
      }
    }

    // Adds space around ones if all ones...
    var allOnes = clues.All(c => c.Length == 1);
    if (allOnes) {
      for (var i = 0; i < length; ++i) {
        if (cellStates[i] == PicrossCellState.KNOWN_FILLED) {
          if (i > 0 && cellStates[i - 1] == PicrossCellState.UNKNOWN) {
            yield return (PicrossMoveType.MARK_EMPTY,
                          PicrossMoveSource.EMPTY_AROUND_ONES,
                          i - 1);
          }

          if (i < length - 1 && cellStates[i + 1] == PicrossCellState.UNKNOWN) {
            yield return (PicrossMoveType.MARK_EMPTY,
                          PicrossMoveSource.EMPTY_AROUND_ONES,
                          i + 1);
          }
        }
      }
    }
  }

  private class PicrossClue(byte length) {
    public byte Length => length;
    public bool Used { get; set; }
  }

  private static IReadOnlyList<IReadOnlyList<PicrossClue>> ToMutableClues_(
      IReadOnlyList<IReadOnlyList<byte>> clues)
    => clues.Select(t => t.Select(v => new PicrossClue(v)).ToArray()).ToArray();

  private static IEnumerable<PicrossMove1d> TryToFitCluesIntoGaps_(
      IReadOnlyList<PicrossCellState> cellStates,
      IReadOnlyList<PicrossClue> clues,
      int[] clueIndices,
      bool forward) {
    Array.Fill(clueIndices, -1);

    var length = cellStates.Count;
    GetStepValues_(forward,
                   length,
                   out var iStart,
                   out var iEnd,
                   out var increment);
    GetStepValues_(forward,
                   clues.Count,
                   out var clueStart,
                   out var clueEnd,
                   out _);

    var totalUnknownCount = 0;

    PicrossClue? currentClue = null;

    var isOnlyClue = clues.Count == 1;

    var i = iStart;
    for (var clueI = clueStart; clueI != clueEnd; clueI += increment) {
      var clue = clues[clueI];

      var isFirstClue = clueI == clueStart;

      RetryClue:
      var clueUnknownCount = 0;
      int? hitKnownFilledI = null;
      var clueLength = clue.Length;
      for (var clueCellI = 0; clueCellI < clueLength; ++clueCellI) {
        var cellState = cellStates[i + increment * clueCellI];

        // Uh oh, not expecting to reach a known empty cell. Can't start here.
        if (cellState == PicrossCellState.KNOWN_EMPTY) {
          // If this was the first clue and there's no room, we need to mark as
          // empty.
          if (isFirstClue) {
            for (var badClueCellI = 0;
                 badClueCellI < clueCellI - 1;
                 ++badClueCellI) {
              yield return (PicrossMoveType.MARK_EMPTY,
                            PicrossMoveSource.END_DOESNT_FIT,
                            i + increment * badClueCellI);
            }
          }

          // Skip ahead.
          i += increment * (clueCellI + 1);
          goto RetryClue;
        }

        // If find one that's marked, we can fill the ones after.
        if (cellState == PicrossCellState.KNOWN_FILLED) {
          hitKnownFilledI ??= clueCellI;
        } else {
          ++clueUnknownCount;
        }
      }

      // If cell after clue is filled, we need to start at least one later.
      var afterClueI = i + increment * clueLength;
      var isAfterClueIOnBoard = afterClueI.IsInRange(0, length - 1);
      if (clueI == clueStart && isAfterClueIOnBoard) {
        if (cellStates[afterClueI] == PicrossCellState.KNOWN_FILLED) {
          yield return (PicrossMoveType.MARK_EMPTY,
                        PicrossMoveSource.TOO_FAR_FROM_KNOWN,
                        i);
          i += increment;
          goto RetryClue;
        }
      }

      // Otherwise, valid clue location! Let's mark this down.

      // If we hit a known cell, we can fill in the cells after!
      if (totalUnknownCount == 0) {
        if (hitKnownFilledI != null) {
          for (var ii = hitKnownFilledI.Value; ii < clueLength; ++ii) {
            var cellI = i + increment * ii;
            if (cellStates[cellI] == PicrossCellState.UNKNOWN) {
              yield return (PicrossMoveType.MARK_FILLED,
                            PicrossMoveSource.NOWHERE_ELSE_TO_GO,
                            cellI);
            }
          }

          // We can mark some cells as empty after if this is the only clue.
          if (isOnlyClue) {
            var unaccountedLength = hitKnownFilledI.Value;
            for (var ii = afterClueI + increment * unaccountedLength;
                 ii != iEnd && ii.IsInRange(0, length - 1);
                 ii += increment) {
              if (cellStates[ii] == PicrossCellState.UNKNOWN) {
                yield return (PicrossMoveType.MARK_EMPTY,
                              PicrossMoveSource.TOO_FAR_FROM_KNOWN, 
                              ii);
              }
            }
          }
        }

        if (clueUnknownCount == 0 && isAfterClueIOnBoard) {
          if (cellStates[afterClueI] == PicrossCellState.UNKNOWN) {
            yield return (PicrossMoveType.MARK_EMPTY,
                          PicrossMoveSource.EMPTY_BETWEEN_CLUES,
                          afterClueI);
          }
        }
      }

      totalUnknownCount += clueUnknownCount;

      for (var clueCellI = 0; clueCellI < clueLength; ++clueCellI) {
        clueIndices[i + increment * clueCellI] = 2 * clueI;
      }

      if (isAfterClueIOnBoard) {
        clueIndices[afterClueI] = 2 * clueI + increment;
      }

      i = afterClueI + increment;
    }
  }

  private static void GetStepValues_(bool forward,
                                     int length,
                                     out int start,
                                     out int end,
                                     out int increment) {
    if (forward) {
      start = 0;
      end = length;
      increment = 1;
      return;
    }

    start = length - 1;
    end = -1;
    increment = -1;
  }
}