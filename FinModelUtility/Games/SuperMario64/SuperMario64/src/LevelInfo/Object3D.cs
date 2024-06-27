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
      m_data = new ObjectData();
    }

    public Object3D(
        string address,
        ObjectData objectData
    ) {
      m_data = objectData;
      Address = address;
      UpdateProperties();
    }

    public void ReplaceData(ObjectData newData) {
      m_data = newData;
      UpdateProperties();
    }

    private ObjectData m_data = new ObjectData();

    [Browsable(false)]
    public ObjectData Data {
      get => m_data;
    }

    [Browsable(false)]
    public bool canEditModelID {
      get { return !isModelIDReadOnly; }
    }

    [Browsable(false)]
    public bool canEditBehavior {
      get { return !isBehaviorReadOnly; }
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
      get { return m_data.ModelId; }
      set { m_data.ModelId = value; }
    }

    [CustomSortedCategory("Model", 2, NUM_OF_CATERGORIES)]
    [Browsable(false)]
    [Description("Model identifer used by the object")]
    [DisplayName("Model ID")]
    [ReadOnly(true)]
    public byte ModelID_ReadOnly {
      get { return m_data.ModelId; }
    }

    [CustomSortedCategory("Position", 3, NUM_OF_CATERGORIES)]
    [Browsable(true)]
    [DisplayName("X")]
    public short xPos {
      get { return m_data.X; }
      set { m_data.X = value; }
    }

    [CustomSortedCategory("Position", 3, NUM_OF_CATERGORIES)]
    [Browsable(true)]
    [DisplayName("Y")]
    public short yPos {
      get { return m_data.Y; }
      set { m_data.Y = value; }
    }

    [CustomSortedCategory("Position", 3, NUM_OF_CATERGORIES)]
    [Browsable(true)]
    [DisplayName("Z")]
    public short zPos {
      get { return m_data.Z; }
      set { m_data.Z = value; }
    }

    [CustomSortedCategory("Rotation", 4, NUM_OF_CATERGORIES)]
    [Browsable(true)]
    [DisplayName("RX")]
    public short xRot {
      get { return m_data.RX; }
      set { m_data.RX = value; }
    }

    [CustomSortedCategory("Rotation", 4, NUM_OF_CATERGORIES)]
    [Browsable(true)]
    [DisplayName("RY")]
    public short yRot {
      get { return m_data.RY; }
      set { m_data.RY = value; }
    }

    [CustomSortedCategory("Rotation", 4, NUM_OF_CATERGORIES)]
    [Browsable(true)]
    [DisplayName("RZ")]
    public short zRot {
      get { return m_data.RZ; }
      set { m_data.RZ = value; }
    }

    [CustomSortedCategory("Behavior", 5, NUM_OF_CATERGORIES)]
    [Browsable(true)]
    [DisplayName("Behavior")]
    public string Behavior {
      get => "0x" + m_data.Behaviour.ToString("X8");
      set {
        m_data.Behaviour =
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
                                   .getBehaviorNameEntryFromSegAddress(
                                       m_data.Behaviour)
                                   .Name;

    public string ToString() => this.Behavior_Name;

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
      get { return m_data.BehaviourArgs[0]; }
      set { m_data.BehaviourArgs[0] = value; }
    }

    [CustomSortedCategory("Behavior", 5, NUM_OF_CATERGORIES)]
    [Browsable(true)]
    [DisplayName(BP2DNAME)]
    [Description("")]
    public byte BehaviorParameter2 {
      get { return m_data.BehaviourArgs[1]; }
      set { m_data.BehaviourArgs[1] = value; }
    }

    [CustomSortedCategory("Behavior", 5, NUM_OF_CATERGORIES)]
    [Browsable(true)]
    [DisplayName(BP3DNAME)]
    [Description("")]
    public byte BehaviorParameter3 {
      get { return m_data.BehaviourArgs[2]; }
      set { m_data.BehaviourArgs[2] = value; }
    }

    [CustomSortedCategory("Behavior", 5, NUM_OF_CATERGORIES)]
    [Browsable(true)]
    [DisplayName(BP4DNAME)]
    [Description("")]
    public byte BehaviorParameter4 {
      get { return m_data.BehaviourArgs[3]; }
      set { m_data.BehaviourArgs[3] = value; }
    }

    [CustomSortedCategory("Acts", 6, NUM_OF_CATERGORIES)]
    [Browsable(true)]
    [DisplayName("All Acts")]
    public bool AllActs {
      get { return m_data.AllActs; }
      set { m_data.AllActs = value; }
    }

    [CustomSortedCategory("Acts", 6, NUM_OF_CATERGORIES)]
    [Browsable(true)]
    [DisplayName("Act 1")]
    public bool Act1 {
      get { return m_data.Acts[0]; }
      set { m_data.Acts[0] = value; }
    }

    [CustomSortedCategory("Acts", 6, NUM_OF_CATERGORIES)]
    [Browsable(true)]
    [DisplayName("Act 2")]
    public bool Act2 {
      get { return m_data.Acts[1]; }
      set { m_data.Acts[1] = value; }
    }

    [CustomSortedCategory("Acts", 6, NUM_OF_CATERGORIES)]
    [Browsable(true)]
    [DisplayName("Act 3")]
    public bool Act3 {
      get { return m_data.Acts[2]; }
      set { m_data.Acts[2] = value; }
    }

    [CustomSortedCategory("Acts", 6, NUM_OF_CATERGORIES)]
    [Browsable(true)]
    [DisplayName("Act 4")]
    public bool Act4 {
      get { return m_data.Acts[3]; }
      set { m_data.Acts[3] = value; }
    }

    [CustomSortedCategory("Acts", 6, NUM_OF_CATERGORIES)]
    [Browsable(true)]
    [DisplayName("Act 5")]
    public bool Act5 {
      get { return m_data.Acts[4]; }
      set { m_data.Acts[4] = value; }
    }

    [CustomSortedCategory("Acts", 6, NUM_OF_CATERGORIES)]
    [Browsable(true)]
    [DisplayName("Act 6")]
    public bool Act6 {
      get { return m_data.Acts[5]; }
      set { m_data.Acts[5] = value; }
    }

    private ulong Flags = 0;

    private bool isReadOnly = false;

    [CustomSortedCategory("Misc", NUM_OF_CATERGORIES, NUM_OF_CATERGORIES)]
    [DisplayName("Read-Only")]
    [Browsable(false)]
    public bool IsReadOnly {
      get { return isReadOnly; }
    }

    /**************************************************************************************/

    [Browsable(false)]
    public Level level { get; set; }

    private ObjectComboEntry objectComboEntry;
    private ushort presetID;

    public int getROMAddress() {
      return int.Parse(Address.Substring(2), NumberStyles.HexNumber);
    }

    public uint getROMUnsignedAddress() {
      return uint.Parse(Address.Substring(2), NumberStyles.HexNumber);
    }

    public void setPresetID(ushort ID) {
      presetID = ID;
    }

    public byte getActMask() {
      byte actMask = 0;
      if (Act1) actMask |= 0x1;
      if (Act2) actMask |= 0x2;
      if (Act3) actMask |= 0x4;
      if (Act4) actMask |= 0x8;
      if (Act5) actMask |= 0x10;
      if (Act6) actMask |= 0x20;
      return actMask;
    }

    public void setBehaviorFromAddress(uint address) {
      m_data.Behaviour = address;
    }

    public uint getBehaviorAddress() => m_data.Behaviour;

    public List<ScriptDumpCommandInfo> ParseBehavior() {
      var script = new List<ScriptDumpCommandInfo>();
      BehaviorScripts.parse(ref script, this.getBehaviorAddress());
      return script;
    }

    public void MakeBehaviorReadOnly(bool isReadOnly) {
      isBehaviorReadOnly = isReadOnly;
    }

    public void MakeModelIDReadOnly(bool isReadOnly) {
      isModelIDReadOnly = isReadOnly;
    }

    public void MakeReadOnly() {
      TypeDescriptor.AddAttributes(
          this,
          [new ReadOnlyAttribute(true)]);
      isReadOnly = true;
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
      return (Flags & (ulong) flag) == (ulong) flag;
    }

    private void updateProperty(string property, FLAGS flag) {
      if (isHidden(flag))
        HideShowProperty(property, false);
      else
        HideShowProperty(property, true);
    }

    private void updateReadOnlyProperty(string property, bool isReadOnly) {
      if (isReadOnly) {
        HideShowProperty(property, false);
        HideShowProperty(property + "_ReadOnly", true);
      } else {
        HideShowProperty(property, true);
        HideShowProperty(property + "_ReadOnly", false);
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
        ChangePropertyName(property, oce_name);
      else
        ChangePropertyName(property, otherName);
    }

    private void UpdatePropertyDescription(string property, string oce_desc) {
      if (oce_desc != null && !oce_desc.Equals(""))
        ChangePropertyDescription(property, oce_desc);
      else
        ChangePropertyDescription(property, "");
    }

    private void UpdateObjectComboNames() {
      if (objectComboEntry != null) {
        UpdatePropertyName("BehaviorParameter1", objectComboEntry.BP1_NAME,
                           BP1DNAME);
        UpdatePropertyName("BehaviorParameter2", objectComboEntry.BP2_NAME,
                           BP2DNAME);
        UpdatePropertyName("BehaviorParameter3", objectComboEntry.BP3_NAME,
                           BP3DNAME);
        UpdatePropertyName("BehaviorParameter4", objectComboEntry.BP4_NAME,
                           BP4DNAME);
        UpdatePropertyDescription("BehaviorParameter1",
                                  objectComboEntry.BP1_DESCRIPTION);
        UpdatePropertyDescription("BehaviorParameter2",
                                  objectComboEntry.BP2_DESCRIPTION);
        UpdatePropertyDescription("BehaviorParameter3",
                                  objectComboEntry.BP3_DESCRIPTION);
        UpdatePropertyDescription("BehaviorParameter4",
                                  objectComboEntry.BP4_DESCRIPTION);
      } else {
        ChangePropertyName("BehaviorParameter1", BP1DNAME);
        ChangePropertyName("BehaviorParameter2", BP2DNAME);
        ChangePropertyName("BehaviorParameter3", BP3DNAME);
        ChangePropertyName("BehaviorParameter4", BP4DNAME);
        ChangePropertyDescription("BehaviorParameter1", "");
        ChangePropertyDescription("BehaviorParameter2", "");
        ChangePropertyDescription("BehaviorParameter3", "");
        ChangePropertyDescription("BehaviorParameter4", "");
      }
    }

    public void UpdateProperties() {
      updateProperty("xPos", FLAGS.POSITION_X);
      updateProperty("yPos", FLAGS.POSITION_Y);
      updateProperty("zPos", FLAGS.POSITION_Z);
      updateProperty("xRot", FLAGS.ROTATION_X);
      updateProperty("yRot", FLAGS.ROTATION_Y);
      updateProperty("zRot", FLAGS.ROTATION_Z);
      updateProperty("Act1", FLAGS.ACT1);
      updateProperty("Act2", FLAGS.ACT2);
      updateProperty("Act3", FLAGS.ACT3);
      updateProperty("Act4", FLAGS.ACT4);
      updateProperty("Act5", FLAGS.ACT5);
      updateProperty("Act6", FLAGS.ACT6);
      updateProperty("AllActs", FLAGS.ALLACTS);
      updateProperty("BehaviorParameter1", FLAGS.BPARAM_1);
      updateProperty("BehaviorParameter2", FLAGS.BPARAM_2);
      updateProperty("BehaviorParameter3", FLAGS.BPARAM_3);
      updateProperty("BehaviorParameter4", FLAGS.BPARAM_4);
      updateReadOnlyProperty("Behavior", isBehaviorReadOnly);
      updateReadOnlyProperty("ModelID", isModelIDReadOnly);
      UpdateObjectComboNames();
    }

    FLAGS tempHideFlags;

    bool isBehaviorReadOnly_tempTrigger = false,
         isModelIDReadOnly_tempTrigger = false;

    public void HideFieldsTemporarly(FLAGS showFlags) {
      tempHideFlags = (~showFlags & ~(FLAGS) Flags) & FLAGS.ALLFLAGS;
      //Console.WriteLine(Convert.ToString((int)Flags, 2).PadLeft(32, '0'));
      //Console.WriteLine(Convert.ToString((int)tempHideFlags, 2).PadLeft(32, '0'));

      isTempHidden = true;
      if (!isBehaviorReadOnly) {
        isBehaviorReadOnly_tempTrigger = true;
        isBehaviorReadOnly = true;
      }
      if (!isModelIDReadOnly) {
        isModelIDReadOnly_tempTrigger = true;
        isModelIDReadOnly = true;
      }
      HideProperty(tempHideFlags);
      UpdateProperties();
    }

    public void RevealTemporaryHiddenFields() {
      if (isTempHidden) {
        if (isBehaviorReadOnly_tempTrigger) {
          isBehaviorReadOnly_tempTrigger = false;
          isBehaviorReadOnly = false;
        }
        if (isModelIDReadOnly_tempTrigger) {
          isModelIDReadOnly_tempTrigger = false;
          isModelIDReadOnly = false;
        }
        UnhideProperty(tempHideFlags);
        UpdateProperties();
        isTempHidden = false;
        tempHideFlags = 0;
      }
    }

    public FLAGS getFlagFromDisplayName(string? displayName) {
      if (displayName == GetPropertyDisplayName("xPos"))
        return FLAGS.POSITION_X;
      if (displayName == GetPropertyDisplayName("yPos"))
        return FLAGS.POSITION_Y;
      if (displayName == GetPropertyDisplayName("zPos"))
        return FLAGS.POSITION_Z;
      if (displayName == GetPropertyDisplayName("xRot"))
        return FLAGS.ROTATION_X;
      if (displayName == GetPropertyDisplayName("yRot"))
        return FLAGS.ROTATION_Y;
      if (displayName == GetPropertyDisplayName("zRot"))
        return FLAGS.ROTATION_Z;
      if (displayName == GetPropertyDisplayName("Act1")) return FLAGS.ACT1;
      if (displayName == GetPropertyDisplayName("Act2")) return FLAGS.ACT2;
      if (displayName == GetPropertyDisplayName("Act3")) return FLAGS.ACT3;
      if (displayName == GetPropertyDisplayName("Act4")) return FLAGS.ACT4;
      if (displayName == GetPropertyDisplayName("Act5")) return FLAGS.ACT5;
      if (displayName == GetPropertyDisplayName("Act6")) return FLAGS.ACT6;
      if (displayName == GetPropertyDisplayName("AllActs"))
        return FLAGS.ALLACTS;
      if (displayName == GetPropertyDisplayName("BehaviorParameter1"))
        return FLAGS.BPARAM_1;
      if (displayName == GetPropertyDisplayName("BehaviorParameter2"))
        return FLAGS.BPARAM_2;
      if (displayName == GetPropertyDisplayName("BehaviorParameter3"))
        return FLAGS.BPARAM_3;
      if (displayName == GetPropertyDisplayName("BehaviorParameter4"))
        return FLAGS.BPARAM_4;
      return 0;
    }

    public void SetBehaviorParametersToZero() {
      BehaviorParameter1 = 0;
      BehaviorParameter2 = 0;
      BehaviorParameter3 = 0;
      BehaviorParameter4 = 0;
    }

    public void DontShowActs() {
      Flags |= (ulong) (
                         FLAGS.ACT1 | FLAGS.ACT2 | FLAGS.ACT3 |
                         FLAGS.ACT4 | FLAGS.ACT5 | FLAGS.ACT6 |
                         FLAGS.ALLACTS);
    }

    public void ShowHideActs(bool hide) {
      if (hide)
        Flags |= (ulong) (FLAGS.ACT1 | FLAGS.ACT2 |
                          FLAGS.ACT3 | FLAGS.ACT4 | FLAGS.ACT5 | FLAGS.ACT6);
      else
        Flags &= ~(ulong) (FLAGS.ACT1 | FLAGS.ACT2 |
                           FLAGS.ACT3 | FLAGS.ACT4 | FLAGS.ACT5 | FLAGS.ACT6);
      UpdateProperties();
    }

    public void HideProperty(FLAGS flag) {
      Flags |= (ulong) flag;
    }

    public void UnhideProperty(FLAGS flag) {
      Flags &= ~(ulong) flag;
    }

    public void renameObjectCombo(string newName) {
      string oldComboName = Title;
      Title = newName;
      bool undefinedToDefined = oldComboName.StartsWith("Undefined Combo (")
                                && !newName.StartsWith("Undefined Combo (");

      if (!undefinedToDefined) // simple re-define
      {
        if (objectComboEntry != null)
          objectComboEntry.Name = newName;
      } else {
        uint modelAddress = 0;
        ObjectComboEntry newOCE =
            new ObjectComboEntry(newName, ModelID, modelAddress,
                                 getBehaviorAddress());
        objectComboEntry = newOCE;
        Globals.objectComboEntries.Add(newOCE);
        level.LevelObjectCombos.Add(newOCE);
      }

      ModelComboFile.writeObjectCombosFile(Globals.getDefaultObjectComboPath());
    }

    public string getObjectComboName() {
      uint behaviorAddr = getBehaviorAddress();
      uint modelSegmentAddress = 0;
      for (int i = 0; i < Globals.objectComboEntries.Count; i++) {
        ObjectComboEntry entry = Globals.objectComboEntries[i];
        modelSegmentAddress = 0;
        if (entry.ModelID == ModelID && entry.Behavior == behaviorAddr
                                     && entry.ModelSegmentAddress ==
                                     modelSegmentAddress) {
          objectComboEntry = entry;
          Title = entry.Name;
          return entry.Name;
        }
      }

      objectComboEntry = null;
      Title = "Undefined Combo (0x" + m_data.ModelId.ToString("X2") + ", 0x" +
              behaviorAddr.ToString("X8") + ")";
      return Title;
    }

    public bool isPropertyShown(FLAGS flag) {
      return !isHidden(flag);
    }
  }
}