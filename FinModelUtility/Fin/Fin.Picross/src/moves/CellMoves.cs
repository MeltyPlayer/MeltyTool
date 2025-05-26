using Generator.Equals;

namespace fin.picross.moves;

public enum PicrossCellMoveType {
  MARK_EMPTY,
  MARK_FILLED
}

public enum PicrossCellMoveSource {
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

[Equatable]
public partial record PicrossCellMove1d(
    PicrossCellMoveType MoveType,
    PicrossCellMoveSource MoveSource,
    int I) : IPicrossMove1d {
  [IgnoreEquality]
  public PicrossCellMoveSource MoveSource { get; } = MoveSource;
}

[Equatable]
public partial record PicrossCellMove(
    PicrossCellMoveType MoveType,
    PicrossCellMoveSource MoveSource,
    int X,
    int Y) : IPicrossMove {
  [IgnoreEquality]
  public PicrossCellMoveSource MoveSource { get; } = MoveSource;
}