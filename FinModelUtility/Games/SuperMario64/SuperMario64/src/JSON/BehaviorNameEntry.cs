namespace sm64.JSON {
  public class BehaviorNameEntry {
    string name = "";
    uint behavior = 0;

    public string Name {
      get { return this.name; }
      set { this.name = value; }
    }

    public uint Behavior {
      get { return this.behavior; }
    }

    public BehaviorNameEntry(string name, uint behavior) {
      this.name = name;
      this.behavior = behavior;
    }

    public override string ToString() {
      return this.name + " = [" + "0x" + this.behavior.ToString("X8") + "]";
    }
  }
}