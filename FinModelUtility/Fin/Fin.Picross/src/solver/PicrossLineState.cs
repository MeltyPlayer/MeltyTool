namespace fin.picross.solver;

public interface IPicrossLineState {
  IReadOnlyList<IReadOnlyPicrossCellState> CellStates { get; }
  IReadOnlyList<IPicrossClueState> Clues { get; }
}

public class PicrossLineState : IPicrossLineState {
  public required IReadOnlyList<IReadOnlyPicrossCellState> CellStates {
    get;
    init;
  }

  public required IReadOnlyList<IPicrossClueState> Clues { get; init; }
}