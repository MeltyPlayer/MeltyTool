using f3dzex2.image;

using sm64.JSON;
using sm64.LevelInfo;
using sm64.memory;
using sm64.Scripts;

namespace sm64.api;

public static class Sm64LevelImporter {
  public static Level LoadLevel(Sm64LevelSceneFileBundle levelFileBundle) {
    ROM rom = ROM.Instance;

    rom.clearSegments();
    rom.readFile(levelFileBundle.Sm64Rom.FullPath);

    Globals.objectComboEntries.Clear();
    Globals.behaviorNameEntries.Clear();
    BehaviorNameFile.parseBehaviorNames(
        Globals.getDefaultBehaviorNamesPath());
    ModelComboFile.parseObjectCombos(Globals.getDefaultObjectComboPath());
    var memoryConstants = Globals.MemoryConstants;
    rom.setSegment(0x15,
                   memoryConstants.Segment15.Offset,
                   memoryConstants.Segment15.Offset + memoryConstants.Segment15.Length,
                   false,
                   null);
    rom.setSegment(0x02,
                   memoryConstants.Segment2.Offset,
                   memoryConstants.Segment2.Offset + memoryConstants.Segment2.Length,
                   rom.isSegmentMIO0(0x02, null),
                   rom.Seg02_isFakeMIO0,
                   rom.Seg02_uncompressedOffset,
                   null);

    var sm64Hardware = new N64Hardware<ISm64Memory>();
    sm64Hardware.Memory = new Sm64Memory();
    sm64Hardware.Rdp = new Rdp { Tmem = new JankTmem(sm64Hardware) };
    sm64Hardware.Rsp = new Rsp();

    var level = new Level((ushort) levelFileBundle.LevelId, 1);
    LevelScripts.parse(sm64Hardware, ref level, 0x15, 0);
    level.sortAndAddNoModelEntries();
    level.CurrentAreaID = level.Areas[0].AreaID;

    return level;
  }
}