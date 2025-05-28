using Generator.Equals;

namespace fin.picross.moves;

public enum PicrossClueMoveSource {
  FREEBIE_EMPTY,
  FREEBIE_FULL_LENGTH,
  FREEBIE_PERFECT_FIT,
  FIRST_CLUE_IN_LINE,
  ALL_CLUES_SOLVED,
  ONLY_MATCHING_CLUE,
  LAST_UNSOLVED_CLUE,
}

[Equatable]
public partial record PicrossClueMove(
    PicrossClueMoveSource MoveSource,
    IPicrossClue Clue,
    int StartIndex) : IPicrossMove1d, IPicrossMove {
  [IgnoreEquality]
  public PicrossClueMoveSource MoveSource { get; } = MoveSource;
}