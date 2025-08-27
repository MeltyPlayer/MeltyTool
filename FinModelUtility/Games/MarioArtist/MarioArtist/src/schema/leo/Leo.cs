namespace marioartist.schema.leo;

/// <summary>
///   Shamelessly stolen from:
///   https://github.com/LuigiBlood/mfs_manager/blob/master/mfs_library/Leo/Leo.cs
/// </summary>
public static class Leo {
  public const int MAX_LBA = 0x10DB;
  public const int SIZE_LBA = MAX_LBA + 1;
  public const int SYSTEM_LBAS = 24;
  public const int DISKID_LBA = 14;

  public const int SECTORS_PER_BLOCK = 85;

  public const int BLOCKS_PER_TRACK = 2;

  public const int DISK_SIZE_MAME = 0x435B0C0;
  public const int DISK_SIZE_SDK = 0x3DEC800;

  /* LBA to Disk System Data (Retail) */
  public static readonly int[] LBA_SYS_PROD = [0, 1, 8, 9];

  /* LBA to Disk System Data (Development) */
  public static readonly int[] LBA_SYS_DEV = [2, 3, 10, 11];

  /* Sector Size in bytes [zone] */
  public static readonly int[] SECTOR_SIZE =
    [232, 216, 208, 192, 176, 160, 144, 128, 112];

  /* Block Size in bytes [zone] */
  public static readonly int[] BLOCK_SIZE = [
    0x4D08, 0x47B8, 0x4510, 0x3FC0, 0x3A70, 0x3520, 0x2FD0, 0x2A80, 0x2530
  ];

  /* Outer Track per Zone [zone] */
  static readonly int[] PZONE_TRACK =
    [0x000, 0x09E, 0x13C, 0x1D1, 0x266, 0x2FB, 0x390, 0x425, 0x497];

  /* Start Track of Zone [pzone] */
  static readonly int[] TRACK_START_ZONE_TBL = [
    0x000, 0x09E, 0x13C, 0x1D1, 0x266, 0x2FB, 0x390, 0x425,
    0x091, 0x12F, 0x1C4, 0x259, 0x2EE, 0x383, 0x418, 0x48A
  ];

  /* LBA to VZone [type,vzone] */
  static readonly int[,] VZONE_LBA_TBL = {
    {
      0x0124, 0x0248, 0x035A, 0x047E, 0x05A2, 0x06B4, 0x07C6, 0x08D8,
      0x09EA, 0x0AB6, 0x0B82, 0x0C94, 0x0DA6, 0x0EB8, 0x0FCA, 0x10DC
    }, {
      0x0124, 0x0248, 0x035A, 0x046C, 0x057E, 0x06A2, 0x07C6, 0x08D8,
      0x09EA, 0x0AFC, 0x0BC8, 0x0C94, 0x0DA6, 0x0EB8, 0x0FCA, 0x10DC
    }, {
      0x0124, 0x0248, 0x035A, 0x046C, 0x057E, 0x0690, 0x07A2, 0x08C6,
      0x09EA, 0x0AFC, 0x0C0E, 0x0CDA, 0x0DA6, 0x0EB8, 0x0FCA, 0x10DC
    }, {
      0x0124, 0x0248, 0x035A, 0x046C, 0x057E, 0x0690, 0x07A2, 0x08B4,
      0x09C6, 0x0AEA, 0x0C0E, 0x0D20, 0x0DEC, 0x0EB8, 0x0FCA, 0x10DC
    }, {
      0x0124, 0x0248, 0x035A, 0x046C, 0x057E, 0x0690, 0x07A2, 0x08B4,
      0x09C6, 0x0AD8, 0x0BEA, 0x0D0E, 0x0E32, 0x0EFE, 0x0FCA, 0x10DC
    }, {
      0x0124, 0x0248, 0x035A, 0x046C, 0x057E, 0x0690, 0x07A2, 0x086E,
      0x0980, 0x0A92, 0x0BA4, 0x0CB6, 0x0DC8, 0x0EEC, 0x1010, 0x10DC
    }, {
      0x0124, 0x0248, 0x035A, 0x046C, 0x057E, 0x0690, 0x07A2, 0x086E,
      0x093A, 0x0A4C, 0x0B5E, 0x0C70, 0x0D82, 0x0E94, 0x0FB8, 0x10DC
    }
  };

  /* VZone to PZone [type,vzone] */
  static readonly int[,] VZONE_PZONE_TBL = {
    {
      0x0, 0x1, 0x2, 0x9, 0x8, 0x3, 0x4, 0x5, 0x6, 0x7, 0xF, 0xE, 0xD, 0xC,
      0xB, 0xA
    }, {
      0x0, 0x1, 0x2, 0x3, 0xA, 0x9, 0x8, 0x4, 0x5, 0x6, 0x7, 0xF, 0xE, 0xD,
      0xC, 0xB
    }, {
      0x0, 0x1, 0x2, 0x3, 0x4, 0xB, 0xA, 0x9, 0x8, 0x5, 0x6, 0x7, 0xF, 0xE,
      0xD, 0xC
    }, {
      0x0, 0x1, 0x2, 0x3, 0x4, 0x5, 0xC, 0xB, 0xA, 0x9, 0x8, 0x6, 0x7, 0xF,
      0xE, 0xD
    }, {
      0x0, 0x1, 0x2, 0x3, 0x4, 0x5, 0x6, 0xD, 0xC, 0xB, 0xA, 0x9, 0x8, 0x7,
      0xF, 0xE
    }, {
      0x0, 0x1, 0x2, 0x3, 0x4, 0x5, 0x6, 0x7, 0xE, 0xD, 0xC, 0xB, 0xA, 0x9,
      0x8, 0xF
    }, {
      0x0, 0x1, 0x2, 0x3, 0x4, 0x5, 0x6, 0x7, 0xF, 0xE, 0xD, 0xC, 0xB, 0xA,
      0x9, 0x8
    }
  };

  /* LBA Start RAM Area [type] */
  public static readonly short[] RamStartLBA =
    [0x5A2, 0x7C6, 0x9EA, 0xC0E, 0xE32, 0x1010, 0x10DC];

  /* RAM Area Total Sizes [type] */
  public static readonly int[] RamSize =
    [0x24A9DC0, 0x1C226C0, 0x1450F00, 0xD35680, 0x6CFD40, 0x1DA240, 0x0];

  /* LBA To Virtual Zone */
  public static int LBAToVZone(int lba, int disktype) {
    for (int vzone = 0; vzone < 16; vzone++) {
      if (lba < VZONE_LBA_TBL[disktype, vzone]) {
        return vzone;
      }
    }

    return -1;
  }

  /* Virtual Zone to Physical Zone */
  public static int VZoneToPZone(int vzone, int disktype)
    => VZONE_PZONE_TBL[disktype, vzone];

  /* Calculate byte size from LBA x to LBA x+y */
  public static int LBAToByte(int disktype, int startlba, int nlbas) {
    if (disktype < 0 && disktype > 7)
      throw new ArgumentOutOfRangeException("Disk Type out of range",
                                            disktype.ToString());
    if (startlba < 0 && startlba > Leo.MAX_LBA)
      throw new ArgumentOutOfRangeException("LBA Start out of range",
                                            startlba.ToString());
    if (nlbas < 0 && nlbas > Leo.MAX_LBA)
      throw new ArgumentOutOfRangeException("LBA Amount out of range",
                                            nlbas.ToString());
    if ((startlba + nlbas) < 0 && (startlba + nlbas) > Leo.MAX_LBA)
      throw new ArgumentOutOfRangeException("LBA Start out of range",
                                            (startlba + nlbas).ToString());

    int totalbytes = 0;
    bool init_flag = true;
    int vzone = 1;
    int pzone = 0;
    int lba = startlba;
    int lba_count = nlbas;
    int blkbytes = 0;
    if (nlbas != 0) {
      for (; lba_count != 0; lba_count--) {
        if ((init_flag) || (VZONE_LBA_TBL[disktype, vzone] == lba)) {
          vzone = LBAToVZone(lba, disktype);
          pzone = VZoneToPZone(vzone, disktype);
          if (7 < pzone) {
            pzone -= 7;
          }

          blkbytes = BLOCK_SIZE[pzone];
        }

        totalbytes += blkbytes;
        lba++;
        init_flag = false;
        if ((lba_count > 1) && (lba > MAX_LBA)) {
          return -1;
        }
      }
    }

    return totalbytes;
  }

  /* MAME Disk Format Offsets for each zone */
  static readonly int[] MAMEOffsetTable = [
    0x0, 0x5F15E0, 0xB79D00, 0x10801A0, 0x1523720, 0x1963D80, 0x1D414C0,
    0x20BBCE0,
    0x23196E0, 0x28A1E00, 0x2DF5DC0, 0x3299340, 0x36D99A0, 0x3AB70E0,
    0x3E31900, 0x4149200
  ];

  public static int LBAToMAMEOffset(int lba, ReadOnlySpan<byte> sysData) {
    if (lba < 0 && lba > Leo.MAX_LBA)
      throw new ArgumentOutOfRangeException("LBA out of range", lba.ToString());

    int head, track, block;
    LBAToPhys(lba, sysData, out head, out track, out block);
    return PhysToMAMEOffset(head, track, block, 0);
  }

  public static int
    PhysToMAMEOffset(int head, int track, int block, int sector) {
    int pzone = Array.FindIndex(PZONE_TRACK, x => track < x) - 1;

    int trackRelative = track - PZONE_TRACK[pzone];

    int offsetCalc = MAMEOffsetTable[pzone + (head * 8)];
    offsetCalc += BLOCK_SIZE[pzone + head] * 2 * trackRelative;
    offsetCalc += block * BLOCK_SIZE[pzone + head];
    offsetCalc += sector * SECTOR_SIZE[pzone + head];

    if (offsetCalc >= DISK_SIZE_MAME)
      throw new ArgumentOutOfRangeException("Offset Result is out of bounds.",
                                            "0x" + offsetCalc.ToString("X"));

    return offsetCalc;
  }

  public static void LBAToPhys(int lba,
                               ReadOnlySpan<byte> sysData,
                               out int head,
                               out int track,
                               out int block) {
    if (lba < 0 && lba > Leo.MAX_LBA)
      throw new ArgumentOutOfRangeException("LBA out of range", lba.ToString());

    //Get Disk Type
    int diskType = sysData[0x05] & 0x0F;

    //Get Block
    if (((lba & 3) == 0) || ((lba & 3) == 3))
      block = 0;
    else
      block = 1;

    //Calculate Head & Track
    int VZone = LBAToVZone(lba, diskType);
    int PZone = VZoneToPZone(VZone, diskType);

    //Get Head
    head = (7 < PZone) ? 1 : 0;

    int calcPZone = PZone - (head * 7);

    int calcTrack
      = (lba - ((VZone == 0) ? 0 : VZONE_LBA_TBL[diskType, VZone - 1])) /
        BLOCKS_PER_TRACK;

    //int zoneTrackStart = PZONE_TRACK[calcPZone - head];
    int zoneTrackStart = TRACK_START_ZONE_TBL[PZone];
    if (head == 1) {
      calcTrack = -calcTrack;
      zoneTrackStart = PZONE_TRACK[calcPZone - head];
    }

    calcTrack += TRACK_START_ZONE_TBL[PZone];

    int calcDefectOffset = (PZone == 0) ? 0 : sysData[8 + PZone - 1];
    int calcDefectAmount = sysData[8 + PZone] - calcDefectOffset;

    while ((calcDefectAmount > 0) &&
           ((sysData[0x20 + calcDefectOffset] + zoneTrackStart) <= calcTrack)) {
      calcTrack++;
      calcDefectOffset++;
      calcDefectAmount--;
    }

    track = calcTrack;
  }
}