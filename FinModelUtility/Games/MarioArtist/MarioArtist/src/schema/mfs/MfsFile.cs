using schema.binary;
using schema.binary.attributes;

namespace marioartist.schema.mfs;

/// <summary>
///   Shamelessly stolen from:
///   https://github.com/LuigiBlood/mfs_manager/blob/master/mfs_library/MFS/MFSDef.cs#L107
/// </summary>
[BinarySchema]
public partial class MfsFile : IBinaryDeserializable, IMfsEntry {
  public MfsEntryFlags Flags { get; set; }
  public ushort ParentDirectoryIndex { get; set; }

  [Skip]
  public string CompanyCode { get; set; }

  [Skip]
  public string GameCode { get; set; }

  [ReadLogic]
  private void ReadCompanyCodeAndGameCode_(IBinaryReader br) {
    this.CompanyCode = SjisUtil.ReadString(br, 2);
    this.GameCode = SjisUtil.ReadString(br, 4);
  }

  public ushort FatEntry { get; set; }

  public uint Size { get; set; }

  [Skip]
  public string Name { get; set; }

  [Skip]
  public string Ext { get; set; }

  [ReadLogic]
  private void ReadNameAndExt_(IBinaryReader br) {
    this.Name = SjisUtil.ReadString(br, 20);
    this.Ext = SjisUtil.ReadString(br, 5);
  }

  public byte CopyNb { get; set; }

  public byte Renewal { get; set; }
  public byte Unk3 { get; set; }

  public MfsDate Date { get; } = new();
}