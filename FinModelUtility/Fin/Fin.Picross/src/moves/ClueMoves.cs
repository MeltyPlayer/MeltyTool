using Generator.Equals;

namespace fin.picross.moves;

public enum PicrossClueMoveSource {
  FREEBIE_EMPTY,
  FREEBIE_FULL_LENGTH,
  FREEBIE_PERFECT_FIT,
  FIRST_CLUE,
  ALL_CLUES_SOLVED,
}

[Equatable]
public partial record PicrossClueMove(
    PicrossClueMoveSource MoveSource,
    IPicrossClue Clue) : IPicrossMove1d, IPicrossMove;