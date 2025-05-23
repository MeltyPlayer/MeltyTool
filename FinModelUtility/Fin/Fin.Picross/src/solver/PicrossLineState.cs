namespace fin.picross.solver;

public interface IPicrossLineState {
  IReadOnlyList<IReadOnlyPicrossCellState> CellStates { get; }
  IReadOnlyList<PicrossClueState> Clues { get; }
}

public class PicrossLineState : IPicrossLineState {
  public required IReadOnlyList<IReadOnlyPicrossCellState> CellStates {
    get;
    init;
  }

  public required IReadOnlyList<PicrossClueState> Clues { get; init; }
}