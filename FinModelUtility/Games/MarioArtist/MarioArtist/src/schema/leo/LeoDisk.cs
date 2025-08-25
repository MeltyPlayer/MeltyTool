using System.Security.Cryptography;
using System.Text;

using fin.util.asserts;

using marioartist.schema.mfs;

using schema.binary;

namespace marioartist.schema.leo;

/// <summary>
/// Shamelessly stolen from:
/// https://github.com/LuigiBlood/mfs_manager/blob/master/mfs_library/Leo/LeoDisk.cs
/// </summary>
public class LeoDisk {
  public enum DiskFormat {
    SDK,
    D64,
    RAM,
    N64,
    MAME,
    Invalid
  }

  public enum FileSystem {
    MFS,
    ATNFS,
    Invalid
  }

  public DiskFormat Format;
  public FileSystem RAMFileSystem;
  public int OffsetToRamArea;
  public int OffsetToSysData;
  public int DiskType;
  public byte[] Data;

  public LeoDisk(IBinaryReader br) {
    this.Load(br);
  }

  void Load(IBinaryReader br) {
    //Assume file is bad first
    this.Format = DiskFormat.Invalid;
    this.RAMFileSystem = FileSystem.Invalid;
    this.OffsetToSysData = -1;
    this.OffsetToRamArea = -1;

    var fileLength = br.Length;

    if (fileLength > Leo.RamSize[0]) {
      //Perform System Area heuristics if the file size is MAME or SDK
      bool correctSysData = false;
      byte[] sysData = new byte[Leo.SECTOR_SIZE[0]];
      if ((fileLength == Leo.DISK_SIZE_MAME) ||
          (fileLength == Leo.DISK_SIZE_SDK)) {
        //Check each System Data Block

        //Check Retail SysData
        foreach (int lba in Leo.LBA_SYS_PROD) {
          this.OffsetToSysData = Leo.BLOCK_SIZE[0] * lba;
          br.Position = this.OffsetToSysData;
          br.ReadBytes(sysData);

          bool isEqual = true;
          for (int i = 1; i < Leo.SECTORS_PER_BLOCK; i++) {
            byte[] sysDataCompare = new byte[sysData.Length];
            br.ReadBytes(sysDataCompare);

            //Compare Bytes
            for (int j = 0; j < sysDataCompare.Length; j++) {
              if (sysDataCompare[j] != sysData[j]) {
                isEqual = false;
                break;
              }
            }

            //If not equal then don't bother doing more
            if (!isEqual) break;
          }

          correctSysData = isEqual;

          //If SysData is found then it's fine don't bother checking the rest
          if (correctSysData) break;
        }

        //Check Dev SysData if not found
        if (!correctSysData) {
          sysData = new byte[Leo.SECTOR_SIZE[3]];
          foreach (int lba in Leo.LBA_SYS_DEV) {
            this.OffsetToSysData = Leo.BLOCK_SIZE[0] * lba;
            br.Position = this.OffsetToSysData;
            br.ReadBytes(sysData);

            bool isEqual = true;
            for (int i = 1; i < Leo.SECTORS_PER_BLOCK; i++) {
              byte[] sysDataCompare = new byte[sysData.Length];
              br.ReadBytes(sysDataCompare);

              //Compare Bytes
              for (int j = 0; j < sysDataCompare.Length; j++) {
                if (sysDataCompare[j] != sysData[j]) {
                  isEqual = false;
                  break;
                }
              }

              //If not equal then don't bother doing more
              if (!isEqual) break;
            }

            correctSysData = isEqual;

            //If SysData is found then it's fine don't bother checking the rest
            if (correctSysData) break;
          }
        }
      }

      if (fileLength == Leo.DISK_SIZE_MAME) {
        /* --- Check if it's MAME Format --- */

        //if SysData found
        if (correctSysData) {
          this.DiskType = sysData[0x5] & 0xF;
          this.Format = DiskFormat.MAME;
          this.OffsetToRamArea = 0;
          //Data is good.
        }
      } else if (fileLength == Leo.DISK_SIZE_SDK) {
        /* --- Check if it's SDK Format --- */

        //if SysData found
        if (correctSysData) {
          this.DiskType = sysData[0x5] & 0xF;
          this.Format = DiskFormat.SDK;
          this.OffsetToRamArea
              = Leo.LBAToByte(this.DiskType, 0, Leo.RamStartLBA[this.DiskType]);
          //Data is good.
        }
      } else {
        /* --- Check if it's N64 CART Format --- */

        //SHA256 check if N64 Cartridge Port bootloader
        byte[] headerTest = new byte[0xFC0];
        br.Position = 0x40;
        br.ReadBytes(headerTest);

        SHA256 hashHeader = SHA256.Create();
        hashHeader.ComputeHash(headerTest);

        string hashHeaderStr = "";
        foreach (byte b in hashHeader.Hash)
          hashHeaderStr += b.ToString("x2");

        Console.WriteLine(hashHeaderStr);

        int offsetStart = 0;

        //SHA256 = 53c0088fb777870d0af32f0251e964030e2e8b72e830c26042fd191169508c05
        if (hashHeaderStr ==
            "53c0088fb777870d0af32f0251e964030e2e8b72e830c26042fd191169508c05") {
          offsetStart
              = 0x738C0 - 0x10E8; //Start of User LBA 0 (24 w/ System Area)

          br.Position = 0x1000;
          br.ReadBytes(sysData);

          this.DiskType = sysData[0x5] & 0xF;
          this.Format = DiskFormat.N64;
          this.OffsetToRamArea
              = Leo.LBAToByte(this.DiskType,
                              0,
                              Leo.RamStartLBA[this.DiskType]) -
                offsetStart;
          this.OffsetToSysData = 0x1000;
          //Data is good.
        }
      }
    } else {
      /* --- Check if it's RAM Format --- */
      if (Array.Exists(Leo.RamSize, x => x == fileLength)) {
        this.DiskType = Array.FindIndex(Leo.RamSize, x => x == fileLength);
        this.Format = DiskFormat.RAM;
        this.OffsetToRamArea = 0;
        //Data is good.
      }
    }

    if (this.Format != DiskFormat.Invalid) {
      //Copy full file
      this.Data = new byte[fileLength];
      br.Position = 0;
      br.ReadBytes(this.Data);
      //Disk is considered loaded here.
    }

    /* Check RAM FileSystem */
    if (this.Format != DiskFormat.Invalid) {
      //Only check if RAM Area exists (Disk Type 6 has no RAM area)
      if (this.DiskType < 6) {
        //MultiFileSystem
        byte[] test = new byte[Mfs.RAM_ID.Length];
        byte[] firstRAM = this.ReadLBA(Leo.RamStartLBA[this.DiskType])
                              .AssertNonnull();
        Array.Copy(firstRAM, test, test.Length);

        //See if equal to RAM_ID, and if so, it is found.
        if (Encoding.ASCII.GetString(test).Equals(Mfs.RAM_ID)) {
          this.RAMFileSystem = FileSystem.MFS;
        }
      }
    }
  }

  public void Save(string filepath) {
    FileStream file = new FileStream(filepath, FileMode.Create);
    file.Write(this.Data, 0, this.Data.Length);
    file.Close();
  }

  public byte[]? ReadLBA(int lba) {
    //Do not read anywhere before RAM Area
    if (lba < Leo.RamStartLBA[this.DiskType]) return null;
    if (lba > Leo.MAX_LBA) return null;

    //Read Block
    byte[] output = new byte[Leo.LBAToByte(this.DiskType, lba, 1)];
    if (this.Format == DiskFormat.MAME) {
      int sourceOffset = Leo.LBAToMAMEOffset(lba, this.GetSystemData().AssertNonnull());
      Array.Copy(this.Data, sourceOffset, output, 0, output.Length);
    } else {
      if (this.OffsetToRamArea < 0) return null;
      int sourceOffset
          = Leo.LBAToByte(this.DiskType,
                          Leo.RamStartLBA[this.DiskType],
                          lba - Leo.RamStartLBA[this.DiskType]) +
            this.OffsetToRamArea;
      Array.Copy(this.Data, sourceOffset, output, 0, output.Length);
    }

    return output;
  }

  public byte[]? GetSystemData() {
    if (this.OffsetToSysData < 0) return null;

    byte[] sysData = new byte[Leo.SECTOR_SIZE[0]];

    Array.Copy(this.Data, this.OffsetToSysData, sysData, 0, sysData.Length);

    return sysData;
  }

  public byte[]? GetRAMAreaArray() {
    if (this.OffsetToRamArea < 0) return null;

    List<byte> array = new List<byte>();

    for (int lba = Leo.RamStartLBA[this.DiskType]; lba <= Leo.MAX_LBA; lba++) {
      array.AddRange(this.ReadLBA(lba).AssertNonnull());
    }

    return array.ToArray();
  }
}