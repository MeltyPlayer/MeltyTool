namespace sm64.JSON;

public class ObjectComboEntry {
  string name = "";
  string behavior_name = "";
  ModelCombo modelCombo;
  uint behavior = 0;

  public string Name {
    get { return name; }
    set { name = value; }
  }

  public string BehaviorName {
    get { return behavior_name; }
    set { behavior_name = value; }
  }

  public byte ModelID {
    get { return modelCombo.ModelID; }
  }

  public uint ModelSegmentAddress {
    get { return modelCombo.SegmentAddress; }
  }

  public uint Behavior {
    get { return behavior; }
  }

  public ObjectComboEntry(string name,
                          byte modelId,
                          uint modelSegAddress,
                          uint behavior) {
      this.name = name;
      this.behavior = behavior;
      behavior_name = Globals.getBehaviorNameEntryFromSegAddress(behavior).Name;
      modelCombo = new ModelCombo(modelId, modelSegAddress);
    }

  private string bp1, bp2, bp3, bp4;

  public string BP1_NAME {
    get { return bp1; }
    set { bp1 = value; }
  }

  public string BP2_NAME {
    get { return bp2; }
    set { bp2 = value; }
  }

  public string BP3_NAME {
    get { return bp3; }
    set { bp3 = value; }
  }

  public string BP4_NAME {
    get { return bp4; }
    set { bp4 = value; }
  }

  private string bp1_desc, bp2_desc, bp3_desc, bp4_desc;

  public string BP1_DESCRIPTION {
    get { return bp1_desc; }
    set { bp1_desc = value; }
  }

  public string BP2_DESCRIPTION {
    get { return bp2_desc; }
    set { bp2_desc = value; }
  }

  public string BP3_DESCRIPTION {
    get { return bp3_desc; }
    set { bp3_desc = value; }
  }

  public string BP4_DESCRIPTION {
    get { return bp4_desc; }
    set { bp4_desc = value; }
  }

  public override string ToString() {
      return name + " = [" + modelCombo.ToString() + "," + behavior_name +
             " (0x" + behavior.ToString("X8") + ")]";
    }
}