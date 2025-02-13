using System.ComponentModel;
using System.Globalization;
using System.Reflection;

using sm64.JSON;
using sm64.LevelInfo;
using sm64.Scripts;

namespace sm64 {
  public class Object3D {
    public enum FLAGS {
      POSITION_X = 0x1,
      POSITION_Y = 0x2,
      POSITION_Z = 0x4,
      ROTATION_X = 0x8,
      ROTATION_Y = 0x10,
      ROTATION_Z = 0x20,
      ACT1 = 0x40,
      ACT2 = 0x80,
      ACT3 = 0x100,
      ACT4 = 0x200,
      ACT5 = 0x400,
      ACT6 = 0x800,
      ALLACTS = 0x1000,
      BPARAM_1 = 0x2000,
      BPARAM_2 = 0x4000,
      BPARAM_3 = 0x8000,
      BPARAM_4 = 0x10000,
      ALLFLAGS = 0x1FFFF
    }

    public enum FROM_LS_CMD {
      CMD_24,
      CMD_39,
      CMD_2E_8,
      CMD_2E_10,
      CMD_2E_12
    }

    private const ushort NUM_OF_CATERGORIES = 7;

    bool isBehaviorReadOnly = false;
    bool isModelIDReadOnly = false;
    bool isTempHidden = false;

    public Object3D() {
      this.m_data = new ObjectData();
    }

    public Object3D(
        string address,
        ObjectData objectData
    ) {
      this.m_data = objectData;
      this.Address = address;
      this.UpdateProperties();
    }

    public void ReplaceData(ObjectData newData) {
      this.m_data = newData;
      this.UpdateProperties();
    }

    private ObjectData m_data = new ObjectData();

    [Browsable(false)]
    public ObjectData Data {
      get => this.m_data;
    }

    [Browsable(false)]
    public bool canEditModelID {
      get { return !this.isModelIDReadOnly; }
    }

    [Browsable(false)]
    public bool canEditBehavior {
      get { return !this.isBehaviorReadOnly; }
    }

    [Browsable(false)]
    public FROM_LS_CMD createdFromLevelScriptCommand { get; set; }

    [CustomSortedCategory("Info", 1, NUM_OF_CATERGORIES)]
    [Browsable(true)]
    [Description("Name of the object combo")]
    [DisplayName("Combo Name")]
    [ReadOnly(true)]
    public string Title { get; set; }

    [CustomSortedCategory("Info", 1, NUM_OF_CATERGORIES)]
    [Browsable(true)]
    [Description("Location inside the ROM file")]
    [DisplayName("Address")]
    [ReadOnly(true)]
    public string Address { get; set; }

    [CustomSortedCategory("Model", 2, NUM_OF_CATERGORIES)]
    [Browsable(true)]
    [Description("Model identifer used by the object")]
    [DisplayName("Model ID")]
    public byte ModelID {
      get { return this.m_data.ModelId; }
      set { this.m_data.ModelId = value; }
    }

    [CustomSortedCategory("Model", 2, NUM_OF_CATERGORIES)]
    [Browsable(false)]
    [Description("Model identifer used by the object")]
    [DisplayName("Model ID")]
    [ReadOnly(true)]
    public byte ModelID_ReadOnly {
      get { return this.m_data.ModelId; }
    }

    [CustomSortedCategory("Position", 3, NUM_OF_CATERGORIES)]
    [Browsable(true)]
    [DisplayName("X")]
    public short xPos {
      get { return this.m_data.X; }
      set { this.m_data.X = value; }
    }

    [CustomSortedCategory("Position", 3, NUM_OF_CATERGORIES)]
    [Browsable(true)]
    [DisplayName("Y")]
    public short yPos {
      get { return this.m_data.Y; }
      set { this.m_data.Y = value; }
    }

    [CustomSortedCategory("Position", 3, NUM_OF_CATERGORIES)]
    [Browsable(true)]
    [DisplayName("Z")]
    public short zPos {
      get { return this.m_data.Z; }
      set { this.m_data.Z = value; }
    }

    [CustomSortedCategory("Rotation", 4, NUM_OF_CATERGORIES)]
    [Browsable(true)]
    [DisplayName("RX")]
    public short xRot {
      get { return this.m_data.RX; }
      set { this.m_data.RX = value; }
    }

    [CustomSortedCategory("Rotation", 4, NUM_OF_CATERGORIES)]
    [Browsable(true)]
    [DisplayName("RY")]
    public short yRot {
      get { return this.m_data.RY; }
      set { this.m_data.RY = value; }
    }

    [CustomSortedCategory("Rotation", 4, NUM_OF_CATERGORIES)]
    [Browsable(true)]
    [DisplayName("RZ")]
    public short zRot {
      get { return this.m_data.RZ; }
      set { this.m_data.RZ = value; }
    }

    [CustomSortedCategory("Behavior", 5, NUM_OF_CATERGORIES)]
    [Browsable(true)]
    [DisplayName("Behavior")]
    public string Behavior {
      get => "0x" + this.m_data.Behaviour.ToString("X8");
      set {
        this.m_data.Behaviour =
            uint.Parse(value.Substring(2), NumberStyles.HexNumber);
      }
    }

    [CustomSortedCategory("Behavior", 5, NUM_OF_CATERGORIES)]
    [Browsable(false)]
    [DisplayName("Behavior")]
    [ReadOnly(true)]
    public string Behavior_ReadOnly => this.Behavior;

    [CustomSortedCategory("Behavior", 5, NUM_OF_CATERGORIES)]
    [Browsable(true)]
    [DisplayName("Beh. Name")]
    [ReadOnly(true)]
    public string Behavior_Name => Globals
                                   .getBehaviorNameEntryFromSegAddress(this.m_data.Behaviour)
                                   .Name;

    public new string ToString() => this.Behavior_Name;

    // default names
    private const string BP1DNAME = "B.Param 1";
    private const string BP2DNAME = "B.Param 2";
    private const string BP3DNAME = "B.Param 3";
    private const string BP4DNAME = "B.Param 4";

    [CustomSortedCategory("Behavior", 5, NUM_OF_CATERGORIES)]
    [Browsable(true)]
    [DisplayName(BP1DNAME)]
    [Description("")]
    public byte BehaviorParameter1 {
      get { return this.m_data.BehaviourArgs[0]; }
      set { this.m_data.BehaviourArgs[0] = value; }
    }

    [CustomSortedCategory("Behavior", 5, NUM_OF_CATERGORIES)]
    [Browsable(true)]
    [DisplayName(BP2DNAME)]
    [Description("")]
    public byte BehaviorParameter2 {
      get { return this.m_data.BehaviourArgs[1]; }
      set { this.m_data.BehaviourArgs[1] = value; }
    }

    [CustomSortedCategory("Behavior", 5, NUM_OF_CATERGORIES)]
    [Browsable(true)]
    [DisplayName(BP3DNAME)]
    [Description("")]
    public byte BehaviorParameter3 {
      get { return this.m_data.BehaviourArgs[2]; }
      set { this.m_data.BehaviourArgs[2] = value; }
    }

    [CustomSortedCategory("Behavior", 5, NUM_OF_CATERGORIES)]
    [Browsable(true)]
    [DisplayName(BP4DNAME)]
    [Description("")]
    public byte BehaviorParameter4 {
      get { return this.m_data.BehaviourArgs[3]; }
      set { this.m_data.BehaviourArgs[3] = value; }
    }

    [CustomSortedCategory("Acts", 6, NUM_OF_CATERGORIES)]
    [Browsable(true)]
    [DisplayName("All Acts")]
    public bool AllActs {
      get { return this.m_data.AllActs; }
      set { this.m_data.AllActs = value; }
    }

    [CustomSortedCategory("Acts", 6, NUM_OF_CATERGORIES)]
    [Browsable(true)]
    [DisplayName("Act 1")]
    public bool Act1 {
      get { return this.m_data.Acts[0]; }
      set { this.m_data.Acts[0] = value; }
    }

    [CustomSortedCategory("Acts", 6, NUM_OF_CATERGORIES)]
    [Browsable(true)]
    [DisplayName("Act 2")]
    public bool Act2 {
      get { return this.m_data.Acts[1]; }
      set { this.m_data.Acts[1] = value; }
    }

    [CustomSortedCategory("Acts", 6, NUM_OF_CATERGORIES)]
    [Browsable(true)]
    [DisplayName("Act 3")]
    public bool Act3 {
      get { return this.m_data.Acts[2]; }
      set { this.m_data.Acts[2] = value; }
    }

    [CustomSortedCategory("Acts", 6, NUM_OF_CATERGORIES)]
    [Browsable(true)]
    [DisplayName("Act 4")]
    public bool Act4 {
      get { return this.m_data.Acts[3]; }
      set { this.m_data.Acts[3] = value; }
    }

    [CustomSortedCategory("Acts", 6, NUM_OF_CATERGORIES)]
    [Browsable(true)]
    [DisplayName("Act 5")]
    public bool Act5 {
      get { return this.m_data.Acts[4]; }
      set { this.m_data.Acts[4] = value; }
    }

    [CustomSortedCategory("Acts", 6, NUM_OF_CATERGORIES)]
    [Browsable(true)]
    [DisplayName("Act 6")]
    public bool Act6 {
      get { return this.m_data.Acts[5]; }
      set { this.m_data.Acts[5] = value; }
    }

    private ulong Flags = 0;

    private bool isReadOnly = false;

    [CustomSortedCategory("Misc", NUM_OF_CATERGORIES, NUM_OF_CATERGORIES)]
    [DisplayName("Read-Only")]
    [Browsable(false)]
    public bool IsReadOnly {
      get { return this.isReadOnly; }
    }

    /**************************************************************************************/

    [Browsable(false)]
    public Level level { get; set; }

    private ObjectComboEntry objectComboEntry;
    private ushort presetID;

    public int getROMAddress() {
      return int.Parse(this.Address.Substring(2), NumberStyles.HexNumber);
    }

    public uint getROMUnsignedAddress() {
      return uint.Parse(this.Address.Substring(2), NumberStyles.HexNumber);
    }

    public void setPresetID(ushort ID) {
      this.presetID = ID;
    }

    public byte getActMask() {
      byte actMask = 0;
      if (this.Act1) actMask |= 0x1;
      if (this.Act2) actMask |= 0x2;
      if (this.Act3) actMask |= 0x4;
      if (this.Act4) actMask |= 0x8;
      if (this.Act5) actMask |= 0x10;
      if (this.Act6) actMask |= 0x20;
      return actMask;
    }

    public void setBehaviorFromAddress(uint address) {
      this.m_data.Behaviour = address;
    }

    public uint getBehaviorAddress() => this.m_data.Behaviour;

    public List<ScriptDumpCommandInfo> ParseBehavior() {
      var script = new List<ScriptDumpCommandInfo>();
      BehaviorScripts.parse(ref script, this.getBehaviorAddress());
      return script;
    }

    public void MakeBehaviorReadOnly(bool isReadOnly) {
      this.isBehaviorReadOnly = isReadOnly;
    }

    public void MakeModelIDReadOnly(bool isReadOnly) {
      this.isModelIDReadOnly = isReadOnly;
    }

    public void MakeReadOnly() {
      TypeDescriptor.AddAttributes(
          this,
          [new ReadOnlyAttribute(true)]);
      this.isReadOnly = true;
    }

    private void HideShowProperty(string property, bool show) {
      PropertyDescriptor descriptor =
          TypeDescriptor.GetProperties(this.GetType())[property];
      BrowsableAttribute attrib =
          (BrowsableAttribute) descriptor.Attributes[
              typeof(BrowsableAttribute)];
      FieldInfo isBrow =
          attrib.GetType()
                .GetField("browsable",
                          BindingFlags.NonPublic | BindingFlags.Instance);

      if (isBrow != null)
        isBrow.SetValue(attrib, show);
    }

    private bool isHidden(FLAGS flag) {
      return (this.Flags & (ulong) flag) == (ulong) flag;
    }

    private void updateProperty(string property, FLAGS flag) {
      if (this.isHidden(flag))
        this.HideShowProperty(property, false);
      else
        this.HideShowProperty(property, true);
    }

    private void updateReadOnlyProperty(string property, bool isReadOnly) {
      if (isReadOnly) {
        this.HideShowProperty(property, false);
        this.HideShowProperty(property + "_ReadOnly", true);
      } else {
        this.HideShowProperty(property, true);
        this.HideShowProperty(property + "_ReadOnly", false);
      }
    }

    private void ChangePropertyName(string property, string name) {
      PropertyDescriptor descriptor =
          TypeDescriptor.GetProperties(this.GetType())[property];
      DisplayNameAttribute attrib =
          (DisplayNameAttribute) descriptor.Attributes[
              typeof(DisplayNameAttribute)];
      FieldInfo isBrow =
          attrib.GetType()
                .GetField("_displayName",
                          BindingFlags.NonPublic | BindingFlags.Instance);

      if (isBrow != null)
        isBrow.SetValue(attrib, name);
    }

    private string? GetPropertyDisplayName(string property) {
      PropertyDescriptor descriptor =
          TypeDescriptor.GetProperties(this.GetType())[property];
      DisplayNameAttribute attrib =
          (DisplayNameAttribute) descriptor.Attributes[
              typeof(DisplayNameAttribute)];

      return (string?) attrib.GetType()
                   .GetField("_displayName",
                             BindingFlags.NonPublic |
                             BindingFlags.Instance)
                   ?.GetValue(attrib);
    }

    private void
        ChangePropertyDescription(string property, string description) {
      PropertyDescriptor descriptor =
          TypeDescriptor.GetProperties(this.GetType())[property];
      DescriptionAttribute attrib =
          (DescriptionAttribute) descriptor.Attributes[
              typeof(DescriptionAttribute)];
      FieldInfo isBrow =
          attrib.GetType()
                .GetField("description",
                          BindingFlags.NonPublic | BindingFlags.Instance);
      if (isBrow != null)
        isBrow.SetValue(attrib, description);
    }

    private void UpdatePropertyName(string property,
                                    string oce_name,
                                    string otherName) {
      if (oce_name != null && !oce_name.Equals(""))
        this.ChangePropertyName(property, oce_name);
      else
        this.ChangePropertyName(property, otherName);
    }

    private void UpdatePropertyDescription(string property, string oce_desc) {
      if (oce_desc != null && !oce_desc.Equals(""))
        this.ChangePropertyDescription(property, oce_desc);
      else
        this.ChangePropertyDescription(property, "");
    }

    private void UpdateObjectComboNames() {
      if (this.objectComboEntry != null) {
        this.UpdatePropertyName("BehaviorParameter1",
                                this.objectComboEntry.BP1_NAME,
                                BP1DNAME);
        this.UpdatePropertyName("BehaviorParameter2",
                                this.objectComboEntry.BP2_NAME,
                                BP2DNAME);
        this.UpdatePropertyName("BehaviorParameter3",
                                this.objectComboEntry.BP3_NAME,
                                BP3DNAME);
        this.UpdatePropertyName("BehaviorParameter4",
                                this.objectComboEntry.BP4_NAME,
                                BP4DNAME);
        this.UpdatePropertyDescription("BehaviorParameter1",
                                       this.objectComboEntry.BP1_DESCRIPTION);
        this.UpdatePropertyDescription("BehaviorParameter2",
                                       this.objectComboEntry.BP2_DESCRIPTION);
        this.UpdatePropertyDescription("BehaviorParameter3",
                                       this.objectComboEntry.BP3_DESCRIPTION);
        this.UpdatePropertyDescription("BehaviorParameter4",
                                       this.objectComboEntry.BP4_DESCRIPTION);
      } else {
        this.ChangePropertyName("BehaviorParameter1", BP1DNAME);
        this.ChangePropertyName("BehaviorParameter2", BP2DNAME);
        this.ChangePropertyName("BehaviorParameter3", BP3DNAME);
        this.ChangePropertyName("BehaviorParameter4", BP4DNAME);
        this.ChangePropertyDescription("BehaviorParameter1", "");
        this.ChangePropertyDescription("BehaviorParameter2", "");
        this.ChangePropertyDescription("BehaviorParameter3", "");
        this.ChangePropertyDescription("BehaviorParameter4", "");
      }
    }

    public void UpdateProperties() {
      this.updateProperty("xPos", FLAGS.POSITION_X);
      this.updateProperty("yPos", FLAGS.POSITION_Y);
      this.updateProperty("zPos", FLAGS.POSITION_Z);
      this.updateProperty("xRot", FLAGS.ROTATION_X);
      this.updateProperty("yRot", FLAGS.ROTATION_Y);
      this.updateProperty("zRot", FLAGS.ROTATION_Z);
      this.updateProperty("Act1", FLAGS.ACT1);
      this.updateProperty("Act2", FLAGS.ACT2);
      this.updateProperty("Act3", FLAGS.ACT3);
      this.updateProperty("Act4", FLAGS.ACT4);
      this.updateProperty("Act5", FLAGS.ACT5);
      this.updateProperty("Act6", FLAGS.ACT6);
      this.updateProperty("AllActs", FLAGS.ALLACTS);
      this.updateProperty("BehaviorParameter1", FLAGS.BPARAM_1);
      this.updateProperty("BehaviorParameter2", FLAGS.BPARAM_2);
      this.updateProperty("BehaviorParameter3", FLAGS.BPARAM_3);
      this.updateProperty("BehaviorParameter4", FLAGS.BPARAM_4);
      this.updateReadOnlyProperty("Behavior", this.isBehaviorReadOnly);
      this.updateReadOnlyProperty("ModelID", this.isModelIDReadOnly);
      this.UpdateObjectComboNames();
    }

    FLAGS tempHideFlags;

    bool isBehaviorReadOnly_tempTrigger = false,
         isModelIDReadOnly_tempTrigger = false;

    public void HideFieldsTemporarly(FLAGS showFlags) {
      this.tempHideFlags = (~showFlags & ~(FLAGS) this.Flags) & FLAGS.ALLFLAGS;
      //Console.WriteLine(Convert.ToString((int)Flags, 2).PadLeft(32, '0'));
      //Console.WriteLine(Convert.ToString((int)tempHideFlags, 2).PadLeft(32, '0'));

      this.isTempHidden = true;
      if (!this.isBehaviorReadOnly) {
        this.isBehaviorReadOnly_tempTrigger = true;
        this.isBehaviorReadOnly = true;
      }
      if (!this.isModelIDReadOnly) {
        this.isModelIDReadOnly_tempTrigger = true;
        this.isModelIDReadOnly = true;
      }

      this.HideProperty(this.tempHideFlags);
      this.UpdateProperties();
    }

    public void RevealTemporaryHiddenFields() {
      if (this.isTempHidden) {
        if (this.isBehaviorReadOnly_tempTrigger) {
          this.isBehaviorReadOnly_tempTrigger = false;
          this.isBehaviorReadOnly = false;
        }
        if (this.isModelIDReadOnly_tempTrigger) {
          this.isModelIDReadOnly_tempTrigger = false;
          this.isModelIDReadOnly = false;
        }

        this.UnhideProperty(this.tempHideFlags);
        this.UpdateProperties();
        this.isTempHidden = false;
        this.tempHideFlags = 0;
      }
    }

    public FLAGS getFlagFromDisplayName(string? displayName) {
      if (displayName == this.GetPropertyDisplayName("xPos"))
        return FLAGS.POSITION_X;
      if (displayName == this.GetPropertyDisplayName("yPos"))
        return FLAGS.POSITION_Y;
      if (displayName == this.GetPropertyDisplayName("zPos"))
        return FLAGS.POSITION_Z;
      if (displayName == this.GetPropertyDisplayName("xRot"))
        return FLAGS.ROTATION_X;
      if (displayName == this.GetPropertyDisplayName("yRot"))
        return FLAGS.ROTATION_Y;
      if (displayName == this.GetPropertyDisplayName("zRot"))
        return FLAGS.ROTATION_Z;
      if (displayName == this.GetPropertyDisplayName("Act1")) return FLAGS.ACT1;
      if (displayName == this.GetPropertyDisplayName("Act2")) return FLAGS.ACT2;
      if (displayName == this.GetPropertyDisplayName("Act3")) return FLAGS.ACT3;
      if (displayName == this.GetPropertyDisplayName("Act4")) return FLAGS.ACT4;
      if (displayName == this.GetPropertyDisplayName("Act5")) return FLAGS.ACT5;
      if (displayName == this.GetPropertyDisplayName("Act6")) return FLAGS.ACT6;
      if (displayName == this.GetPropertyDisplayName("AllActs"))
        return FLAGS.ALLACTS;
      if (displayName == this.GetPropertyDisplayName("BehaviorParameter1"))
        return FLAGS.BPARAM_1;
      if (displayName == this.GetPropertyDisplayName("BehaviorParameter2"))
        return FLAGS.BPARAM_2;
      if (displayName == this.GetPropertyDisplayName("BehaviorParameter3"))
        return FLAGS.BPARAM_3;
      if (displayName == this.GetPropertyDisplayName("BehaviorParameter4"))
        return FLAGS.BPARAM_4;
      return 0;
    }

    public void SetBehaviorParametersToZero() {
      this.BehaviorParameter1 = 0;
      this.BehaviorParameter2 = 0;
      this.BehaviorParameter3 = 0;
      this.BehaviorParameter4 = 0;
    }

    public void DontShowActs() {
      this.Flags |= (ulong) (
                         FLAGS.ACT1 | FLAGS.ACT2 | FLAGS.ACT3 |
                         FLAGS.ACT4 | FLAGS.ACT5 | FLAGS.ACT6 |
                         FLAGS.ALLACTS);
    }

    public void ShowHideActs(bool hide) {
      if (hide)
        this.Flags |= (ulong) (FLAGS.ACT1 | FLAGS.ACT2 |
                               FLAGS.ACT3 | FLAGS.ACT4 | FLAGS.ACT5 | FLAGS.ACT6);
      else
        this.Flags &= ~(ulong) (FLAGS.ACT1 | FLAGS.ACT2 |
                                FLAGS.ACT3 | FLAGS.ACT4 | FLAGS.ACT5 | FLAGS.ACT6);
      this.UpdateProperties();
    }

    public void HideProperty(FLAGS flag) {
      this.Flags |= (ulong) flag;
    }

    public void UnhideProperty(FLAGS flag) {
      this.Flags &= ~(ulong) flag;
    }

    public void renameObjectCombo(string newName) {
      string oldComboName = this.Title;
      this.Title = newName;
      bool undefinedToDefined = oldComboName.StartsWith("Undefined Combo (")
                                && !newName.StartsWith("Undefined Combo (");

      if (!undefinedToDefined) // simple re-define
      {
        if (this.objectComboEntry != null)
          this.objectComboEntry.Name = newName;
      } else {
        uint modelAddress = 0;
        ObjectComboEntry newOCE =
            new ObjectComboEntry(newName,
                                 this.ModelID, modelAddress,
                                 this.getBehaviorAddress());
        this.objectComboEntry = newOCE;
        Globals.objectComboEntries.Add(newOCE);
        this.level.LevelObjectCombos.Add(newOCE);
      }

      ModelComboFile.writeObjectCombosFile(Globals.getDefaultObjectComboPath());
    }

    public string getObjectComboName() {
      uint behaviorAddr = this.getBehaviorAddress();
      uint modelSegmentAddress = 0;
      for (int i = 0; i < Globals.objectComboEntries.Count; i++) {
        ObjectComboEntry entry = Globals.objectComboEntries[i];
        modelSegmentAddress = 0;
        if (entry.ModelID == this.ModelID && entry.Behavior == behaviorAddr
                                          && entry.ModelSegmentAddress ==
                                          modelSegmentAddress) {
          this.objectComboEntry = entry;
          this.Title = entry.Name;
          return entry.Name;
        }
      }

      this.objectComboEntry = null;
      this.Title = "Undefined Combo (0x" +
                   this.m_data.ModelId.ToString("X2") + ", 0x" +
                   behaviorAddr.ToString("X8") + ")";
      return this.Title;
    }

    public bool isPropertyShown(FLAGS flag) {
      return !this.isHidden(flag);
    }
  }
}