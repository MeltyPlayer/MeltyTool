namespace fin.picross.solver;

public interface IPicrossClueState {
  IPicrossClue Clue { get; }
  byte Length { get; }
  bool Used { get; set; }
}

public record PicrossClueState(IPicrossClue Clue) {
  public byte Length => this.Clue.Length;
  public bool Used { get; set; }
}