using fin.color;

namespace modl.level {
  public class Objective : BLevelObject {
    public uint Text { get; set; }
    public IColor Color { get; set; }
    public uint Flags { get; set; }
    public Objective? Next { get; set; }
    public GameScriptResource? Script { get; set; }
  }
}
