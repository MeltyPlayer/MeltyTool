namespace sm64.JSON;

public class BehaviorNameEntry {
  string name = "";
  uint behavior = 0;

  public string Name {
    get { return name; }
    set { name = value; }
  }

  public uint Behavior {
    get { return behavior; }
  }

  public BehaviorNameEntry(string name, uint behavior) {
      this.name = name;
      this.behavior = behavior;
    }

  public override string ToString() {
      return name + " = [" + "0x" + behavior.ToString("X8") + "]";
    }
}