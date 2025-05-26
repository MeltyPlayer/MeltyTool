namespace fin.picross.solver;

public interface IPicrossLineState {
  IReadOnlyList<IReadOnlyPicrossCellState> CellStates { get; }
  IReadOnlyList<IReadOnlyPicrossClueState> ClueStates { get; }
}

public class PicrossLineState : IPicrossLineState {
  public required IReadOnlyList<IReadOnlyPicrossCellState> CellStates {
    get;
    init;
  }

  public required IReadOnlyList<IReadOnlyPicrossClueState> ClueStates { get; init; }
}