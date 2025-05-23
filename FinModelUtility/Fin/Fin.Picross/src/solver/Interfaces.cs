using Generator.Equals;

namespace fin.picross.solver;

[Equatable]
public readonly partial record struct PicrossMove1d(
    PicrossMoveType MoveType,
    PicrossMoveSource MoveSource,
    int I) {
  [IgnoreEquality]
  public PicrossMoveSource MoveSource { get; } = MoveSource;
}

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
  NO_CLUES_FIT,
  NOWHERE_ELSE_TO_GO,
  FORWARD_BACKWARD_OVERLAP,
  TOO_FAR_FROM_KNOWN,
  ALL_CLUES_SOLVED,
  EMPTY_BETWEEN_CLUES,
  EMPTY_AROUND_KNOWN_CLUE,
}

public interface IPicrossSolverMethod {
  IEnumerable<PicrossMove1d> TryToFindMoves(IPicrossLineState lineState);
}