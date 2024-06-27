using System.Drawing;

using f3dzex2.image;

using schema.binary;

using sm64.JSON;
using sm64.memory;
using sm64.schema;
using sm64.Scripts;

namespace sm64.LevelInfo {
  public class AreaBackgroundInfo {
    public uint address = 0;
    public ushort id_or_color = 0;
    public bool isEndCakeImage = false;
    public uint romLocation = 0;
    public bool usesFog = false;
    public Color fogColor = Color.White;
    public List<uint> fogColor_romLocation = [];
  }

  public class Area {
    public Level parent;
    private ushort areaID;

    public ushort AreaID {
      get { return areaID; }
    }

    private uint geoLayoutPointer;

    public uint GeometryLayoutPointer {
      get { return geoLayoutPointer; }
    }

    public AreaBackgroundInfo bgInfo = new AreaBackgroundInfo();

    public Model3DLods AreaModel;
    public CollisionMap collision = new CollisionMap();

    public List<Object3D> Objects = [];
    public List<Object3D> MacroObjects = [];
    public List<Object3D> SpecialObjects = [];
    public List<Warp> Warps = [];
    public List<Warp> PaintingWarps = [];
    public List<WarpInstant> InstantWarps = [];

    private byte? defaultTerrainType_;

    public byte DefaultTerrainType {
      get => defaultTerrainType_ ?? 0;
      set {
        if (this.defaultTerrainType_.HasValue) {
          throw new Exception();
        }
        this.defaultTerrainType_ = value;
      }
    }

    public Area(IN64Hardware<ISm64Memory> sm64Hardware, ushort areaID, uint geoLayoutPointer, Level parent) {
      this.AreaModel = new Model3DLods(sm64Hardware);
      this.areaID = areaID;
      this.geoLayoutPointer = geoLayoutPointer;
      this.parent = parent;
    }
  }

  public class Level {
    private ushort levelID;

    public ushort LevelID {
      get { return levelID; }
    }

    private ushort currentAreaID;

    public ushort CurrentAreaID {
      get { return currentAreaID; }
      set { currentAreaID = value; }
    }

    public List<Area> Areas = [];
    public AreaBackgroundInfo temp_bgInfo = new AreaBackgroundInfo();

    public Dictionary<ushort, Model3DLods> ModelIDs =
        new Dictionary<ushort, Model3DLods>();

    public List<ObjectComboEntry> LevelObjectCombos =
        [];

    public List<PresetMacroEntry> MacroObjectPresets =
        [];

    public List<PresetMacroEntry> SpecialObjectPresets_8 =
        [];

    public List<PresetMacroEntry> SpecialObjectPresets_10 =
        [];

    public List<PresetMacroEntry> SpecialObjectPresets_12 =
        [];

    public List<ScriptDumpCommandInfo> LevelScriptCommands_ForDump =
        [];


    public ObjectComboEntry? getObjectComboFromData(
        byte modelID,
        uint modelAddress,
        uint behavior,
        out int index) {
      for (int i = 0; i < LevelObjectCombos.Count; i++) {
        ObjectComboEntry oce = LevelObjectCombos[i];
        if (oce.ModelID == modelID && oce.ModelSegmentAddress == modelAddress
                                   && oce.Behavior == behavior) {
          index = i;
          return oce;
        }
      }
      index = -1;
      return null;
    }

    private void AddMacroObjectEntries() {
      MacroObjectPresets.Clear();
      ROM rom = ROM.Instance;

      using var br = new SchemaBinaryReader(rom.Bytes);
      br.Position = Globals.MemoryConstants.MacroPresetTable;

      ushort pID = 0x1F;
      for (int i = 0; i < 366; i++) {
        var presetMacroEntry = br.ReadNew<PresetMacroEntry>();
        presetMacroEntry.PresetId = pID++;
        this.MacroObjectPresets.Add(presetMacroEntry);
      }
    }

    public void AddSpecialObjectPreset_8(ushort presetID,
                                         byte modelId,
                                         uint behavior) {
      SpecialObjectPresets_8.Add(
          new PresetMacroEntry(presetID, modelId, behavior));
    }

    public void AddSpecialObjectPreset_10(ushort presetID,
                                          byte modelId,
                                          uint behavior) {
      SpecialObjectPresets_10.Add(
          new PresetMacroEntry(presetID, modelId, behavior));
    }

    public void AddSpecialObjectPreset_12(ushort presetID,
                                          byte modelId,
                                          uint behavior,
                                          byte bp1,
                                          byte bp2) {
      SpecialObjectPresets_12.Add(
          new PresetMacroEntry(presetID, modelId, behavior, bp1, bp2));
    }

    public void AddObjectCombos(byte modelId, uint modelSegAddress) {
      for (int i = 0; i < Globals.objectComboEntries.Count; i++) {
        ObjectComboEntry oce = Globals.objectComboEntries[i];
        if (oce.ModelID == modelId &&
            oce.ModelSegmentAddress == modelSegAddress)
          LevelObjectCombos.Add(oce);
      }
    }

    public void sortAndAddNoModelEntries() {
      for (int i = 0; i < Globals.objectComboEntries.Count; i++) {
        ObjectComboEntry oce = Globals.objectComboEntries[i];
        if (oce.ModelID == 0x00)
          LevelObjectCombos.Add(oce);
      }
      LevelObjectCombos.Sort((x, y) => string.Compare(x.Name, y.Name));
    }

    public Area getCurrentArea() {
      foreach (Area a in Areas)
        if (a.AreaID == currentAreaID)
          return a;
      return Areas[0]; // return default area
    }

    public void setAreaBackgroundInfo(ref Area area) {
      area.bgInfo.address = temp_bgInfo.address;
      area.bgInfo.id_or_color = temp_bgInfo.id_or_color;
      area.bgInfo.isEndCakeImage = temp_bgInfo.isEndCakeImage;
      area.bgInfo.romLocation = temp_bgInfo.romLocation;
      area.bgInfo.usesFog = temp_bgInfo.usesFog;
      area.bgInfo.fogColor = temp_bgInfo.fogColor;
      area.bgInfo.fogColor_romLocation = temp_bgInfo.fogColor_romLocation;
    }

    public Level(ushort levelID, ushort startArea) {
      ROM.Instance.clearSegments();
      this.levelID = levelID;
      currentAreaID = startArea;
      LevelObjectCombos.Clear();
      LevelScriptCommands_ForDump.Clear();
      AddMacroObjectEntries();
    }
  }
}