using schema.readOnly;

namespace fin.picross.solver;

[GenerateReadOnly]
public partial interface IPicrossClueState {
  IPicrossClue Clue { get; }
  byte Length { get; }
  bool Used => this.StartIndex != null;
  int? StartIndex { get; set; }
}

public record PicrossClueState(IPicrossClue Clue) : IPicrossClueState {
  public byte Length => this.Clue.Length;
  public int? StartIndex { get; set; }
}