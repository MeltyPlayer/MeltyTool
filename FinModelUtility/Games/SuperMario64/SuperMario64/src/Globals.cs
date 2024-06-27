using sm64.JSON;
using sm64.memory;
using sm64.Scripts;

namespace sm64 {
  public class ScriptDumpCommandInfo {
    public byte[] data;
    public uint romAddress, segAddress;
    public string description;

    public BehaviorCommand Command => (BehaviorCommand) this.data[0];

    public string ToString() => this.description;
  }

  public class Globals {
    public static bool DEBUG_PLG = false; // parsing level geometry flag
    public static uint DEBUG_PDL = 0x00000000; // parsing display list value
    public static bool DEBUG_PARSING_LEVEL_AREA = false;
    public static bool DEBUG_PARSING_DL = false;

    public static MemoryConstants MemoryConstants { get; set; }

    // RAM to ROM conversion for assembly functions in each region
    public static uint RAMtoROM_JP = 0x80245000; // Japan
    public static uint RAMtoROM_NA = 0x80245000; // USA 
    public static uint RAMtoROM_EU = 0x80240800; // Europe
    public static uint RAMtoROM_JS = 0x80248000; // Japan (Shindou)
    public static uint RAMtoROM_IQ = 0x80248000; // Chinese (IQue)

    // Function location that initalizes segment 0x02 for each region
    public static uint seg02_init_JP = 0x80248934; // Japan
    public static uint seg02_init_NA = 0x80248964; // USA 
    public static uint seg02_init_EU = 0x80244100; // Europe
    public static uint seg02_init_JS = 0x8024B958; // Japan (Shindou)
    public static uint seg02_init_IQ = 0x8024B968; // Chinese (IQue)


    // Function that draws skybox image:
    public static uint skybox_drawFunc_NA = 0x802763D4;

    public static uint getSkyboxDrawFunction() {
      switch (ROM.Instance.Region) {
        default:
          return skybox_drawFunc_NA;
      }
    }

    // Function that draws the ending cake image:
    public static uint endCake_drawFunc_NA = 0x802D28CC;

    public static List<ObjectComboEntry> objectComboEntries =
        [];

    public static List<BehaviorNameEntry> behaviorNameEntries =
        [];

    public static BehaviorNameEntry getBehaviorNameEntryFromSegAddress(
        uint address) {
      foreach (BehaviorNameEntry entry in behaviorNameEntries) {
        if (entry.Behavior == address)
          return entry;
      }

      BehaviorNameEntry new_bne =
          new BehaviorNameEntry("Undefined Behavior", address);
      behaviorNameEntries.Add(new_bne);

      return behaviorNameEntries[behaviorNameEntries.Count - 1];
    }

    public static string getDefaultObjectComboPath() {
      // Console.WriteLine("ROM.Instance.Region = " + ROM.Instance.Region.ToString());
      switch (ROM.Instance.Region) {
        default:
        case ROM_Region.NORTH_AMERICA:
          return "./data/ObjectCombos_NA.json";
        case ROM_Region.EUROPE:
          return "./data/ObjectCombos_EU.json";
        case ROM_Region.JAPAN:
          return "./data/ObjectCombos_JP.json";
        case ROM_Region.JAPAN_SHINDOU:
          return "./data/ObjectCombos_JS.json";
        case ROM_Region.CHINESE_IQUE:
          return "./data/ObjectCombos_IQ.json";
      }
    }

    public static string getDefaultBehaviorNamesPath() {
      // Console.WriteLine("ROM.Instance.Region = " + ROM.Instance.Region.ToString());
      switch (ROM.Instance.Region) {
        default:
          //case ROM_Region.NORTH_AMERICA:
          return "./data/BehaviorNames.json";
        /*case ROM_Region.EUROPE:
            return "./data/ObjectCombos_EU.json";
        case ROM_Region.JAPAN:
            return "./data/ObjectCombos_JP.json";
        case ROM_Region.JAPAN_SHINDOU:
            return "./data/ObjectCombos_JS.json";*/
      }
    }
  }
}