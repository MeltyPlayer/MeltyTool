namespace sm64.JSON {
  public class ObjectComboEntry {
    string name = "";
    string behavior_name = "";
    ModelCombo modelCombo;
    uint behavior = 0;

    public string Name {
      get { return this.name; }
      set { this.name = value; }
    }

    public string BehaviorName {
      get { return this.behavior_name; }
      set { this.behavior_name = value; }
    }

    public byte ModelID {
      get { return this.modelCombo.ModelID; }
    }

    public uint ModelSegmentAddress {
      get { return this.modelCombo.SegmentAddress; }
    }

    public uint Behavior {
      get { return this.behavior; }
    }

    public ObjectComboEntry(string name,
                            byte modelId,
                            uint modelSegAddress,
                            uint behavior) {
      this.name = name;
      this.behavior = behavior;
      this.behavior_name = Globals.getBehaviorNameEntryFromSegAddress(behavior).Name;
      this.modelCombo = new ModelCombo(modelId, modelSegAddress);
    }

    private string bp1, bp2, bp3, bp4;

    public string BP1_NAME {
      get { return this.bp1; }
      set { this.bp1 = value; }
    }

    public string BP2_NAME {
      get { return this.bp2; }
      set { this.bp2 = value; }
    }

    public string BP3_NAME {
      get { return this.bp3; }
      set { this.bp3 = value; }
    }

    public string BP4_NAME {
      get { return this.bp4; }
      set { this.bp4 = value; }
    }

    private string bp1_desc, bp2_desc, bp3_desc, bp4_desc;

    public string BP1_DESCRIPTION {
      get { return this.bp1_desc; }
      set { this.bp1_desc = value; }
    }

    public string BP2_DESCRIPTION {
      get { return this.bp2_desc; }
      set { this.bp2_desc = value; }
    }

    public string BP3_DESCRIPTION {
      get { return this.bp3_desc; }
      set { this.bp3_desc = value; }
    }

    public string BP4_DESCRIPTION {
      get { return this.bp4_desc; }
      set { this.bp4_desc = value; }
    }

    public override string ToString() {
      return this.name + " = [" +
             this.modelCombo.ToString() + "," +
             this.behavior_name +
             " (0x" +
             this.behavior.ToString("X8") + ")]";
    }
  }
}