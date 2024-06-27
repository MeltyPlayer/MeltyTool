using f3dzex2.io;

using LIBMIO0;

using sm64.memory;
using sm64.src;

namespace sm64 {
  public enum ROM_Region {
    JAPAN,
    JAPAN_SHINDOU,
    NORTH_AMERICA,
    EUROPE,
    CHINESE_IQUE
  };

  public enum ROM_Type {
    VANILLA, // 8MB Compressed ROM
    EXTENDED // Uncompressed ROM
  };

  public enum ROM_Endian {
    BIG, // .z64
    LITTLE, // .n64
    MIXED // .v64
  };

  public class ROM {
    private static ROM? instance; // Singleton

    public static ROM Instance => instance ??= new ROM();

    public string Filepath { get; set; } = "";

    private byte[] writeMask;

    //private uint[] segStart = new uint[0x20];
    //private bool[] segIsMIO0 = new bool[0x20];
    //private byte[][] segData = new byte[0x20][];
    private Dictionary<byte, SegBank> segData = new Dictionary<byte, SegBank>();

    private Dictionary<byte, Dictionary<byte, SegBank>> areaSegData = new();

    public uint Seg02_uncompressedOffset { get; private set; } = 0;

    public bool Seg02_isFakeMIO0 { get; private set; } = false;

    public ROM_Region Region { get; private set; } = ROM_Region.NORTH_AMERICA;

    public ROM_Endian Endian { get; private set; } = ROM_Endian.BIG;

    public ROM_Type Type { get; private set; } = ROM_Type.VANILLA;

    public byte[] Bytes { get; private set; }

    private void checkROM() {
      if (this.Bytes[0] == 0x80 && this.Bytes[1] == 0x37)
        this.Endian = ROM_Endian.BIG;
      else if (this.Bytes[0] == 0x37 && this.Bytes[1] == 0x80)
        this.Endian = ROM_Endian.MIXED;
      else if (this.Bytes[0] == 0x40 && this.Bytes[1] == 0x12)
        this.Endian = ROM_Endian.LITTLE;

      if (this.Endian == ROM_Endian.MIXED)
        swapMixedBig();
      else if (this.Endian == ROM_Endian.LITTLE)
        swapLittleBig();

      if (this.Bytes[0x3E] == 0x45)
        this.Region = ROM_Region.NORTH_AMERICA;
      else if (this.Bytes[0x3E] == 0x50)
        this.Region = ROM_Region.EUROPE;
      else if (this.Bytes[0x3E] == 0x4A) {
        if (this.Bytes[0x3F] < 3)
          this.Region = ROM_Region.JAPAN;
        else
          this.Region = ROM_Region.JAPAN_SHINDOU;
      } else if (this.Bytes[0x3E] == 0x00) {
        this.Region = ROM_Region.CHINESE_IQUE;
      }

      Globals.MemoryConstants = this.GetMemoryConstantsForRegion_(this.Region);

      findAndSetSegment02();

      if (this.Bytes[Globals.MemoryConstants.Segment15.Offset] == 0x17)
        this.Type = ROM_Type.EXTENDED;
      else
        this.Type = ROM_Type.VANILLA;
    }

    private MemoryConstants GetMemoryConstantsForRegion_(ROM_Region region) {
      switch (region) {
        case ROM_Region.NORTH_AMERICA: {
          var segment15Start = readWordUnsigned(0x2A622C);
          var segment15End = readWordUnsigned(0x2A6230);
          return MemoryConstants.NA_CONSTANTS with {
              Segment15 = new Segment {
                  Offset = segment15Start,
                  Length = segment15End - segment15Start,
              }
          };
        }
        default:
          throw new NotImplementedException();
      }
    }

    private void swapMixedBig() {
      for (int i = 0; i < this.Bytes.Length; i += 2) {
        byte temp = this.Bytes[i];
        this.Bytes[i] = this.Bytes[i + 1];
        this.Bytes[i + 1] = temp;

        temp = writeMask[i];
        writeMask[i] = writeMask[i + 1];
        writeMask[i + 1] = temp;
      }
    }

    private void swapLittleBig() {
      byte[] temp = new byte[4];
      for (int i = 0; i < this.Bytes.Length; i += 4) {
        temp[0] = this.Bytes[i + 0];
        temp[1] = this.Bytes[i + 1];
        temp[2] = this.Bytes[i + 2];
        temp[3] = this.Bytes[i + 3];
        this.Bytes[i + 0] = temp[3];
        this.Bytes[i + 1] = temp[2];
        this.Bytes[i + 2] = temp[1];
        this.Bytes[i + 3] = temp[0];

        temp[0] = writeMask[i + 0];
        temp[1] = writeMask[i + 1];
        temp[2] = writeMask[i + 2];
        temp[3] = writeMask[i + 3];
        writeMask[i + 0] = temp[3];
        writeMask[i + 1] = temp[2];
        writeMask[i + 2] = temp[1];
        writeMask[i + 3] = temp[0];
      }
    }

    public void clearSegments() {
      foreach (KeyValuePair<byte, SegBank> kvp in segData.ToArray()) {
        if ((new[] {0x15, 2}).Contains(kvp.Key))
          continue;
        segData.Remove(kvp.Key);
      }
      areaSegData.Clear();
    }

    public void readFile(string filename) {
      this.Filepath = filename;
      this.Bytes = File.ReadAllBytes(filename);
      writeMask = new byte[this.Bytes.Length];
      checkROM();
    }

    public void setSegment(uint index,
                           uint segmentStart,
                           uint segmentEnd,
                           bool isMIO0,
                           byte? areaID) {
      setSegment(index, segmentStart, segmentEnd, isMIO0, false, 0, areaID);
    }

    public void setSegment(uint index,
                           uint segmentStart,
                           uint segmentEnd,
                           bool isMIO0,
                           bool fakeMIO0,
                           uint uncompressedOffset,
                           byte? areaID) {
      if (segmentStart > segmentEnd)
        return;

      SegBank seg = new SegBank();
      seg.SegID = (byte) index;

      if (!isMIO0) {
        seg.SegStart = segmentStart;
        seg.IsMIO0 = false;
        uint size = segmentEnd - segmentStart;
        seg.Data = new byte[size];
        for (uint i = 0; i < size; i++)
          seg.Data[i] = this.Bytes[segmentStart + i];
      } else {
        if (fakeMIO0) {
          seg.SegStart = segmentStart + uncompressedOffset;
          seg.IsMIO0 = false;
        } else {
          seg.IsMIO0 = true;
        }
        seg.Data =
            MIO0.mio0_decode(
                getSubArray_safe(this.Bytes, segmentStart,
                                 segmentEnd - segmentStart))!;
      }

      setSegment(index, seg, areaID);
    }

    private void setSegment(uint index, SegBank seg, byte? areaID) {
      if (areaID != null) {
        if (!areaSegData.ContainsKey(areaID.Value)) {
          Dictionary<byte, SegBank> dic = new Dictionary<byte, SegBank>();
          areaSegData.Add(areaID.Value, dic);
        } else if (areaSegData[areaID.Value].ContainsKey((byte) index)) {
          areaSegData[areaID.Value].Remove((byte) index);
        }
        areaSegData[areaID.Value].Add((byte) index, seg);
      } else {
        if (segData.ContainsKey((byte) index)) {
          segData.Remove((byte) index);
        }
        segData.Add((byte) index, seg);
      }
    }

    public byte[]? getSegment(ushort seg, byte? areaID)
      => GetSegBank(seg, areaID)?.Data;

    private SegBank? GetSegBank(ushort seg, byte? areaID) {
      if (areaID != null && areaSegData.ContainsKey(areaID.Value) &&
          areaSegData[areaID.Value].ContainsKey((byte) (seg))) {
        return areaSegData[areaID.Value][(byte) seg];
      }
      if (this.segData.ContainsKey((byte) seg)) {
        return this.segData[(byte) seg];
      }
      return null;
    }

    public uint decodeSegmentAddress(uint segOffset, byte? areaID) {
      // Console.WriteLine("Decoding segment address: " + segOffset.ToString("X8"));
      byte seg = (byte) (segOffset >> 24);

      if (GetSegBank(seg, areaID).IsMIO0)
        throw new ArgumentException(
            "Cannot decode segment address (0x" + segOffset.ToString("X8") +
            ") from MIO0 data. (decodeSegmentAddress 1)");
      uint off = segOffset & 0x00FFFFFF;
      return GetSegBank(seg, areaID).SegStart + off;
    }

    public uint decodeSegmentAddress(byte segment, uint offset, byte? areaID) {
      SegBank seg = GetSegBank(segment, areaID);

      if (seg.IsMIO0)
        throw new ArgumentException(
            "Cannot decode segment address (0x" + segment.ToString("X2") +
            offset.ToString("X6") +
            ") from MIO0 data. (decodeSegmentAddress 2)");
      return seg.SegStart + offset;
    }

    public uint decodeSegmentAddress_safe(byte segment,
                                          uint offset,
                                          byte? areaID) {
      SegBank seg = GetSegBank(segment, areaID);
      if (seg == null) return 0xFFFFFFFF;

      if (seg.IsMIO0)
        return 0xFFFFFFFF;
      return seg.SegStart + offset;
    }

    public byte[] getSubArray_safe(byte[]? arr, uint offset, long size) {
      if (arr == null || arr.Length <= offset)
        return new byte[size];
      if ((arr.Length - offset) < size)
        size = (arr.Length - offset);
      byte[] newArr = new byte[size];
      Array.Copy(arr, offset, newArr, 0, size);
      return newArr;
    }

    public short readHalfword(uint offset) {
      return (short) (this.Bytes[offset] << 8 | this.Bytes[offset + 1]);
    }

    public ushort readHalfwordUnsigned(uint offset) {
      return (ushort) readHalfword(offset);
    }

    public int readWord(uint offset) {
      return this.Bytes[0 + offset] << 24 | this.Bytes[1 + offset] << 16
                                          | this.Bytes[2 + offset] << 8 |
                                          this.Bytes[3 + offset];
    }

    public uint readWordUnsigned(uint offset) {
      return (uint) (this.Bytes[0 + offset] << 24 | this.Bytes[1 + offset] << 16
                                                  | this.Bytes[2 + offset] << 8 |
                                                  this.Bytes[3 + offset]);
    }

    public bool isSegmentMIO0(byte seg, byte? areaID) {
      SegBank segBank = GetSegBank(seg, areaID);
      if (segBank != null)
        return segBank.IsMIO0;
      else return false;
    }

    public bool testIfMIO0IsFake(uint startAddr, int compOff, int uncompOff) {
      if (uncompOff - compOff == 2) {
        if (readHalfwordUnsigned((uint) (startAddr + compOff)) == 0x0000)
          return true; // Detected fake MIO0 header
      }
      return false;
    }

    public void findAndSetSegment02() {
      AssemblyReader ar = new AssemblyReader();
      List<AssemblyReader.JAL_CALL> func_calls;
      SegBank seg = new SegBank();
      uint seg02_init;
      uint RAMtoROM;
      switch (this.Region) {
        default:
        case ROM_Region.NORTH_AMERICA:
          seg02_init = Globals.seg02_init_NA;
          RAMtoROM = Globals.RAMtoROM_NA;
          break;
        case ROM_Region.EUROPE:
          seg02_init = Globals.seg02_init_EU;
          RAMtoROM = Globals.RAMtoROM_EU;
          break;
        case ROM_Region.JAPAN:
          seg02_init = Globals.seg02_init_JP;
          RAMtoROM = Globals.RAMtoROM_JP;
          break;
        case ROM_Region.JAPAN_SHINDOU:
          seg02_init = Globals.seg02_init_JS;
          RAMtoROM = Globals.RAMtoROM_JS;
          break;
        case ROM_Region.CHINESE_IQUE:
          seg02_init = Globals.seg02_init_IQ;
          RAMtoROM = Globals.RAMtoROM_IQ;
          break;
      }

      func_calls = ar.findJALsInFunction(seg02_init, RAMtoROM);
      for (int i = 0; i < func_calls.Count; i++) {
        if (func_calls[i].a0 == 0x2) {
          Globals.MemoryConstants = Globals.MemoryConstants with {
              Segment2 = new Segment {
                  Offset = func_calls[i].a1,
                  Length = func_calls[i].a2 - func_calls[i].a1,
              }
          };
          if (readWordUnsigned(func_calls[i].a1) == 0x4D494F30) {
            seg.IsMIO0 = true;
            this.Seg02_isFakeMIO0 = testIfMIO0IsFake(
                func_calls[i].a1,
                readWord(func_calls[i].a1 + 0x8),
                readWord(func_calls[i].a1 + 0xC)
            );
            seg.SegStart = func_calls[i].a1;
            this.Seg02_uncompressedOffset = readWordUnsigned(func_calls[i].a1 + 0xC);
          }
        }
      }

      setSegment(0x2, seg, null);
    }

    public Dictionary<string, ushort> levelIDs = new Dictionary<string, ushort> {
        {"[C01] Bob-omb Battlefield", 0x09},
        {"[C02] Whomp's Fortress", 0x18},
        {"[C03] Jolly Roger Bay", 0x0C},
        {"[C04] Cool Cool Mountain", 0x05},
        {"[C05] Big Boo's Haunt", 0x04},
        {"[C06] Hazy Maze Cave", 0x07},
        {"[C07] Lethal Lava Land", 0x16},
        {"[C08] Shifting Sand Land", 0x08},
        {"[C09] Dire Dire Docks", 0x17},
        {"[C10] Snowman's Land", 0x0A},
        {"[C11] Wet Dry World", 0x0B},
        {"[C12] Tall Tall Mountain", 0x24},
        {"[C13] Tiny Huge Island", 0x0D},
        {"[C14] Tick Tock Clock", 0x0E},
        {"[C15] Rainbow Ride", 0x0F},
        {"[OW1] Castle Grounds", 0x10},
        {"[OW2] Inside Castle", 0x06},
        {"[OW3] Castle Courtyard", 0x1A},
        {"[BC1] Bowser Course 1", 0x11},
        {"[BC2] Bowser Course 2", 0x13},
        {"[BC3] Bowser Course 3", 0x15},
        {"[MCL] Metal Cap", 0x1C},
        {"[WCL] Wing Cap", 0x1D},
        {"[VCL] Vanish Cap", 0x12},
        {"[BB1] Bowser Battle 1", 0x1E},
        {"[BB2] Bowser Battle 2", 0x21},
        {"[BB3] Bowser Battle 3", 0x22},
        {"[SC1] Secret Aquarium", 0x14},
        {"[SC2] Rainbow Clouds", 0x1F},
        {"[SC3] End Cake Picture", 0x19},
        {"[SlC] Peach's Secret Slide", 0x1B}
    };
  }

  class SegBank {
    public byte[] Data { get; set; } = null;
    public bool IsMIO0 { get; set; } = false;
    public uint SegStart { get; set; } = 0;
    public byte SegID { get; set; } = 0;
  }
}