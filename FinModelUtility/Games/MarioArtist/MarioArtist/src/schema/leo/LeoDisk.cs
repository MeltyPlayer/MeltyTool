using System.Security.Cryptography;
using System.Text;

using marioartist.schema.mfs;

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

  public string Filename;
  public DiskFormat Format;
  public FileSystem RAMFileSystem;
  public int OffsetToRamArea;
  public int OffsetToSysData;
  public int DiskType;
  public byte[] Data;

  public LeoDisk(string filepath) {
    Load(filepath);
  }

  void Load(string filepath) {
    //Assume file is bad first
    Format = DiskFormat.Invalid;
    RAMFileSystem = FileSystem.Invalid;
    OffsetToSysData = -1;
    OffsetToRamArea = -1;
    if (!File.Exists(filepath)) {
      return;
    }

    FileStream file = new FileStream(filepath, FileMode.Open);

    if (file.Length > Leo.RamSize[0]) {
      //Perform System Area heuristics if the file size is MAME or SDK
      bool correctSysData = false;
      byte[] sysData = new byte[Leo.SECTOR_SIZE[0]];
      if ((file.Length == Leo.DISK_SIZE_MAME) ||
          (file.Length == Leo.DISK_SIZE_SDK)) {
        //Check each System Data Block

        //Check Retail SysData
        foreach (int lba in Leo.LBA_SYS_PROD) {
          OffsetToSysData = Leo.BLOCK_SIZE[0] * lba;
          file.Seek(OffsetToSysData, SeekOrigin.Begin);
          file.Read(sysData, 0, sysData.Length);

          bool isEqual = true;
          for (int i = 1; i < Leo.SECTORS_PER_BLOCK; i++) {
            byte[] sysDataCompare = new byte[sysData.Length];
            file.Read(sysDataCompare, 0, sysDataCompare.Length);

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
            OffsetToSysData = Leo.BLOCK_SIZE[0] * lba;
            file.Seek(OffsetToSysData, SeekOrigin.Begin);
            file.Read(sysData, 0, sysData.Length);

            bool isEqual = true;
            for (int i = 1; i < Leo.SECTORS_PER_BLOCK; i++) {
              byte[] sysDataCompare = new byte[sysData.Length];
              file.Read(sysDataCompare, 0, sysDataCompare.Length);

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

      if (file.Length == Leo.DISK_SIZE_MAME) {
        /* --- Check if it's MAME Format --- */

        //if SysData found
        if (correctSysData) {
          DiskType = sysData[0x5] & 0xF;
          Format = DiskFormat.MAME;
          OffsetToRamArea = 0;
          //Data is good.
        }
      } else if (file.Length == Leo.DISK_SIZE_SDK) {
        /* --- Check if it's SDK Format --- */

        //if SysData found
        if (correctSysData) {
          DiskType = sysData[0x5] & 0xF;
          Format = DiskFormat.SDK;
          OffsetToRamArea
              = Leo.LBAToByte(DiskType, 0, Leo.RamStartLBA[DiskType]);
          //Data is good.
        }
      } else {
        /* --- Check if it's N64 CART Format --- */

        //SHA256 check if N64 Cartridge Port bootloader
        byte[] headerTest = new byte[0xFC0];
        file.Seek(0x40, SeekOrigin.Begin);
        file.Read(headerTest, 0, headerTest.Length);

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

          file.Seek(0x1000, SeekOrigin.Begin);
          file.Read(sysData, 0, sysData.Length);

          DiskType = sysData[0x5] & 0xF;
          Format = DiskFormat.N64;
          OffsetToRamArea
              = Leo.LBAToByte(DiskType, 0, Leo.RamStartLBA[DiskType]) -
                offsetStart;
          OffsetToSysData = 0x1000;
          //Data is good.
        }
      }
    } else {
      /* --- Check if it's RAM Format --- */
      if (Array.Exists(Leo.RamSize, x => x == file.Length)) {
        DiskType = Array.FindIndex(Leo.RamSize, x => x == file.Length);
        Format = DiskFormat.RAM;
        OffsetToRamArea = 0;
        //Data is good.
      }
    }

    if (Format != DiskFormat.Invalid) {
      //Copy full file
      Data = new byte[file.Length];
      file.Seek(0, SeekOrigin.Begin);
      file.Read(Data, 0, Data.Length);
      //Disk is considered loaded here.
    }

    file.Close();

    /* Check RAM FileSystem */
    if (Format != DiskFormat.Invalid) {
      //Only check if RAM Area exists (Disk Type 6 has no RAM area)
      if (DiskType < 6) {
        //MultiFileSystem
        byte[] test = new byte[Mfs.RAM_ID.Length];
        byte[] firstRAM = ReadLBA(Leo.RamStartLBA[DiskType]);
        Array.Copy(firstRAM, test, test.Length);

        //See if equal to RAM_ID, and if so, it is found.
        if (Encoding.ASCII.GetString(test).Equals(Mfs.RAM_ID)) {
          RAMFileSystem = FileSystem.MFS;
        }
      }
    }

    Filename = filepath;
  }

  public void Save(string filepath) {
    FileStream file = new FileStream(filepath, FileMode.Create);
    file.Write(Data, 0, Data.Length);
    file.Close();
  }

  public byte[] ReadLBA(int lba) {
    //Do not read anywhere before RAM Area
    if (lba < Leo.RamStartLBA[DiskType]) return null;
    if (lba > Leo.MAX_LBA) return null;

    //Read Block
    byte[] output = new byte[Leo.LBAToByte(DiskType, lba, 1)];
    if (Format == DiskFormat.MAME) {
      int sourceOffset = Leo.LBAToMAMEOffset(lba, GetSystemData());
      Array.Copy(Data, sourceOffset, output, 0, output.Length);
    } else {
      if (OffsetToRamArea < 0) return null;
      int sourceOffset
          = Leo.LBAToByte(DiskType,
                          Leo.RamStartLBA[DiskType],
                          lba - Leo.RamStartLBA[DiskType]) +
            OffsetToRamArea;
      Array.Copy(Data, sourceOffset, output, 0, output.Length);
    }

    return output;
  }

  public void WriteLBA(int lba, byte[] data) {
    //Do not write anywhere before RAM Area
    if (lba < Leo.RamStartLBA[DiskType]) return;
    if (lba > Leo.MAX_LBA) return;

    //Check if data block is exact size of the expected LBA block size
    int blockSize = Leo.LBAToByte(DiskType, lba, 1);
    if (data.Length != blockSize) return;

    //Write Block
    if (Format == DiskFormat.MAME) {
      int destOffset = Leo.LBAToMAMEOffset(lba, GetSystemData());
      Array.Copy(data, 0, Data, destOffset, blockSize);
    } else {
      if (OffsetToRamArea < 0) return;
      int destOffset
          = Leo.LBAToByte(DiskType,
                          Leo.RamStartLBA[DiskType],
                          lba - Leo.RamStartLBA[DiskType]) +
            OffsetToRamArea;
      Array.Copy(data, 0, Data, destOffset, blockSize);
    }
  }

  public byte[] GetSystemData() {
    if (OffsetToSysData < 0) return null;

    byte[] sysData = new byte[Leo.SECTOR_SIZE[0]];

    Array.Copy(Data, OffsetToSysData, sysData, 0, sysData.Length);

    return sysData;
  }

  public byte[] GetRAMAreaArray() {
    if (OffsetToRamArea < 0) return null;

    List<byte> array = new List<byte>();

    for (int lba = Leo.RamStartLBA[DiskType]; lba <= Leo.MAX_LBA; lba++) {
      array.AddRange(ReadLBA(lba));
    }

    return array.ToArray();
  }
}