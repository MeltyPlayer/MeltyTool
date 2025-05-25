using fin.data;
using fin.math;
using fin.util.asserts;

namespace fin.picross.solver;

public class PicrossSolver {
  public IReadOnlyList<IReadOnlySet<PicrossMove>> Solve(
      IPicrossDefinition picrossDefinition,
      out PicrossBoardState finalBoardState) {
    var boardState = new PicrossBoardState(picrossDefinition);
    var clues = new PicrossCluesGenerator().GenerateClues(picrossDefinition);

    var columnLineStates = ToMutableClues_(clues.Columns)
                           .Select((clues, x) => new PicrossLineState {
                               Clues = clues,
                               CellStates = boardState.GetColumn(x).ToArray(),
                           })
                           .ToArray();
    var rowLineStates = ToMutableClues_(clues.Rows)
                        .Select((clues, y) => new PicrossLineState {
                            Clues = clues,
                            CellStates = boardState.GetRow(y).ToArray(),
                        })
                        .ToArray();

    var width = picrossDefinition.Width;
    var height = picrossDefinition.Height;

    var xClueIndicesForward = new int[width];
    var xClueIndicesBackward = new int[width];
    var yClueIndicesForward = new int[height];
    var yClueIndicesBackward = new int[height];

    var moveSets = new List<IReadOnlySet<PicrossMove>>();
    while (true) {
      var isFirstPass = moveSets.Count == 0;
      var moveSet = new HashSet<PicrossMove>();

      for (var x = 0; x < width; ++x) {
        foreach (var (moveType, moveSource, y) in CheckClues_(
                     isFirstPass,
                     columnLineStates[x],
                     yClueIndicesForward,
                     yClueIndicesBackward)) {
          moveSet.Add(new PicrossMove(moveType, moveSource, x, y));
        }
      }

      for (var y = 0; y < height; ++y) {
        foreach (var (moveType, moveSource, x) in CheckClues_(
                     isFirstPass,
                     rowLineStates[y],
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

  private static readonly IReadOnlyList<IPicrossSolverMethod> SOLVER_METHODS_
      = [
          new AlreadySolvedPicrossSolverMethod(),
          new ExtendFirstClueSolverMethod(),
          new ExtendLastClueSolverMethod(),
          new FillSmallestUnknownsBetweenEmptiesSolverMethod(),
          new GapsAroundBiggestSolverMethod(),
          new GapsBetweenNeighboringCluesSolverMethod(),
          new GapsBetweenNeighboringShortCluesSolverMethod(),
      ];

  private static IEnumerable<PicrossMove1d> CheckClues_(
      bool isFirstPass,
      IPicrossLineState lineState,
      int[] clueIndicesForward,
      int[] clueIndicesBackward) {
    var clues = lineState.Clues;
    var cellStates = lineState.CellStates;
    var length = cellStates.Count;

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
            yield return new PicrossMove1d(
                moveType,
                moveSource,
                i);
          }
        }
      }

      // Freebie (adds up to full width)
      else if ((clues.Sum(c => c.Length) + clues.Count - 1) == length) {
        var lineI = 0;
        for (var c = 0; c < clues.Count; c++) {
          if (c > 0) {
            yield return new PicrossMove1d(
                PicrossMoveType.MARK_EMPTY,
                PicrossMoveSource.FREEBIE_PERFECT_FIT,
                lineI++);
          }

          var clue = clues[c];
          clue.Used = true;
          for (var clueI = 0; clueI < clue.Length; ++clueI) {
            yield return new PicrossMove1d(
                PicrossMoveType.MARK_FILLED,
                PicrossMoveSource.FREEBIE_PERFECT_FIT,
                lineI++);
          }
        }
      }

      yield break;
    }

    // Checks line forward and backward.
    var forwardMoves1d
        = TryToFitCluesIntoGaps_(lineState, clueIndicesForward, true);
    var backwardMoves1d
        = TryToFitCluesIntoGaps_(lineState, clueIndicesBackward, false);

    foreach (var picrossMove1d in forwardMoves1d.Concat(backwardMoves1d)) {
      yield return picrossMove1d;
    }

    // Tries to fill in where clues overlap.
    for (var i = 0; i < length; ++i) {
      var clueIndexForward = clueIndicesForward[i];
      var clueIndexBackward = clueIndicesBackward[i];
      if (clueIndexForward != -1 &&
          clueIndexForward == clueIndexBackward &&
          cellStates[i].Status == PicrossCellStatus.UNKNOWN) {
        if (clueIndexForward % 2 == 0) {
          yield return new PicrossMove1d(
              PicrossMoveType.MARK_FILLED,
              PicrossMoveSource.FORWARD_BACKWARD_OVERLAP,
              i);
        } else {
          yield return new PicrossMove1d(
              PicrossMoveType.MARK_EMPTY,
              PicrossMoveSource.EMPTY_BETWEEN_CLUES,
              i);
        }
      }
    }

    foreach (var solverMethod in SOLVER_METHODS_) {
      var hadAnyOfMethod = false;
      foreach (var move in solverMethod.TryToFindMoves(lineState)) {
        hadAnyOfMethod = true;
        yield return move;
      }

      if (hadAnyOfMethod) {
        yield break;
      }
    }
  }

  private static IReadOnlyList<IReadOnlyList<PicrossClueState>> ToMutableClues_(
      IReadOnlyList<IReadOnlyList<byte>> clues)
    => clues.Select(t => t.Select(v => new PicrossClueState(v)).ToArray())
            .ToArray();

  private static IEnumerable<PicrossMove1d> TryToFitCluesIntoGaps_(
      IPicrossLineState lineState,
      int[] clueIndices,
      bool forward) {
    var clues = lineState.Clues;
    var cellStates = lineState.CellStates;
    var length = cellStates.Count;

    Array.Fill(clueIndices, -1);

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

    PicrossClueState? currentClue = null;

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
        var cellState = cellStates[i + increment * clueCellI].Status;

        // Uh oh, not expecting to reach a known empty cell. Can't start here.
        if (cellState == PicrossCellStatus.KNOWN_EMPTY) {
          // If this was the first clue and there's no room, we need to mark as
          // empty.
          if (isFirstClue) {
            for (var badClueCellI = 0;
                 badClueCellI < clueCellI - 1;
                 ++badClueCellI) {
              yield return new PicrossMove1d(
                  PicrossMoveType.MARK_EMPTY,
                  PicrossMoveSource.NO_CLUES_FIT,
                  i + increment * badClueCellI);
            }
          }

          // Skip ahead.
          i += increment * (clueCellI + 1);
          goto RetryClue;
        }

        // If find one that's marked, we can fill the ones after.
        if (cellState == PicrossCellStatus.KNOWN_FILLED) {
          hitKnownFilledI ??= clueCellI;
        } else {
          ++clueUnknownCount;
        }
      }

      // If cell after clue is filled, we need to start at least one later.
      var afterClueI = i + increment * clueLength;
      var isAfterClueIOnBoard = afterClueI.IsInRange(0, length - 1);
      if (clueI == clueStart && isAfterClueIOnBoard) {
        if (cellStates[afterClueI].Status == PicrossCellStatus.KNOWN_FILLED) {
          yield return new PicrossMove1d(
              PicrossMoveType.MARK_EMPTY,
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
            if (cellStates[cellI].Status == PicrossCellStatus.UNKNOWN) {
              yield return new PicrossMove1d(
                  PicrossMoveType.MARK_FILLED,
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
              if (cellStates[ii].Status == PicrossCellStatus.UNKNOWN) {
                yield return new PicrossMove1d(
                    PicrossMoveType.MARK_EMPTY,
                    PicrossMoveSource.TOO_FAR_FROM_KNOWN,
                    ii);
              }
            }
          }
        }

        if (clueUnknownCount == 0 && isAfterClueIOnBoard) {
          if (cellStates[afterClueI].Status == PicrossCellStatus.UNKNOWN) {
            yield return new PicrossMove1d(
                PicrossMoveType.MARK_EMPTY,
                PicrossMoveSource.EMPTY_BETWEEN_CLUES,
                afterClueI);
          }
        }
      }

      if (isFirstClue && clueUnknownCount == 0) {
        clue.Used = true;
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