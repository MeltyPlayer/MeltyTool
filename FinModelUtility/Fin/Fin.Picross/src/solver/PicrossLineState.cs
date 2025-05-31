namespace fin.picross.solver;

public interface IPicrossLineState {
  bool IsColumn { get; }
  IReadOnlyList<IReadOnlyPicrossCellState> CellStates { get; }
  IReadOnlyList<IReadOnlyPicrossClueState> ClueStates { get; }

  bool IsSolved { get; }
}

public class PicrossLineState : IPicrossLineState {
  public required bool IsColumn { get; init; }

  public required IReadOnlyList<IReadOnlyPicrossCellState> CellStates {
    get;
    init;
  }

  public required IReadOnlyList<IReadOnlyPicrossClueState> ClueStates { get; init; }

  public bool IsSolved => this.ClueStates.All(c => c.Solved);
}