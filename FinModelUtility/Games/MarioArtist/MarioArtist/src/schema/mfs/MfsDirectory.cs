using schema.binary;
using schema.binary.attributes;

namespace marioartist.schema.mfs;

/// <summary>
///   Shamelessly stolen from:
///   https://github.com/LuigiBlood/mfs_manager/blob/master/mfs_library/MFS/MFSDef.cs#L107
/// </summary>
[BinarySchema]
public partial class MfsDirectory : IBinaryDeserializable {
  public MfsEntryFlags Flags { get; set; }
  public ushort ParentDirectoryIndex { get; set; }

  [StringLengthSource(2)]
  public string CompanyCode { get; set; }

  [StringLengthSource(4)]
  public string GameCode { get; set; }

  public ushort DirectoryId { get; set; }

  public uint Unk0 { get; set; }

  [StringEncoding(StringEncodingType.UTF8)]
  [StringLengthSource(20)]
  public string Name { get; set; }

  public uint Unk1 { get; set; }
  public ushort Unk2 { get; set; }

  public byte Renewal { get; set; }
  public byte Unk3 { get; set; }

  public MfsDate Date { get; set; }
}