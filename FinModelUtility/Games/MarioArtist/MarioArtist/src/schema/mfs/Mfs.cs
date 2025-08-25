namespace marioartist.schema.mfs;

/// <summary>
///   Shamelessly stolen from:
///   https://github.com/LuigiBlood/mfs_manager/blob/master/mfs_library/MFS/MFS.cs#L46
/// </summary>
public static class Mfs {
  //MFS Volume
  public const string ROM_ID = "64dd-Multi0201";
  public const string RAM_ID = "64dd-Multi0101";

  //MFS FAT
  public enum FAT : ushort {
    Unused = 0x0000,
    DontManage = 0xFFFD,
    Prohibited = 0xFFFE,
    LastFileBlock = 0xFFFF
  }

  public const int FAT_MAX = 2874;

  //MFS Entry
  public struct EntryAttr {
    public bool CopyLimit;   //Limit Copy
    public bool Encode;      //Encode
    public bool Hidden;      //Hidden
    public bool DisableRead; //Cannot be read by other applications

    public bool
        DisableWrite; //Cannot be written, renamed, or deleted by other applications
  }

  public static readonly ushort[] EntryLimit =
      [899, 814, 729, 644, 559, 474, 0];

  enum Error {
    Good = 0, Argument, Filename, FileNotExist, DiskFull, FileAlreadyExists
  }
}