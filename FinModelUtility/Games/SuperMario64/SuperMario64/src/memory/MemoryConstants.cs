using f3dzex2.io;

namespace sm64.memory;

public struct MemoryConstants {
  public required uint MacroPresetTable { get; init; }
  public required uint SpecialPresetTable { get; init; }
  public required Segment Segment2 { get; set; }
  public required Segment Segment15 { get; init; }


  public static MemoryConstants NA_CONSTANTS { get; } = new() {
      MacroPresetTable = 0xEC7E0,
      SpecialPresetTable = 0xED350,
      Segment2 =
          new Segment { Offset = 0x108A40, Length = 0x114750 - 0x108A40, },
      Segment15 = new Segment { Offset = 0, Length = 0, },
  };

  /*
  else if (this.Region == ROM_Region.EUROPE) {
      Globals.macro_preset_table = 0xBD590;
      Globals.special_preset_table = 0xBE100;
      // Globals.seg02_location = new[] { (uint)0xDE190, (uint)0xE49F0 };
      Globals.seg15_location = new[] {(uint) 0x28CEE0, (uint) 0x28D8F0};
    } else if (this.Region == ROM_Region.JAPAN) {
      Globals.macro_preset_table = 0xEB6D0;
      Globals.special_preset_table = 0xEC240;
      // Globals.seg02_location = new[] { (uint)0x1076D0, (uint)0x112B50 };
      Globals.seg15_location = new[] {(uint) 0x2AA240, (uint) 0x2AAC50};
    } else if (this.Region == ROM_Region.JAPAN_SHINDOU) {
      Globals.macro_preset_table = 0xC8D60;
      Globals.special_preset_table = 0xC98D0;
      //Globals.seg02_location = new[] { (uint)0xE42F0, (uint)0xEF770 };
      Globals.seg15_location = new[] {(uint) 0x286AC0, (uint) 0x2874D0};
    } else if (this.Region == ROM_Region.CHINESE_IQUE) {
      Globals.macro_preset_table = 0xCB220;
      Globals.special_preset_table = 0xCBD90;
      //Globals.seg02_location = new[] { (uint)0xE42F0, (uint)0xEF770 };
      Globals.seg15_location = new[] {(uint) 0x298AE0, (uint) 0x2994F0};
    }
   */
}