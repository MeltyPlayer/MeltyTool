namespace fin.picross.solver;

public class PicrossClueState(byte length) {
  public byte Length => length;
  public bool Used { get; set; }
}