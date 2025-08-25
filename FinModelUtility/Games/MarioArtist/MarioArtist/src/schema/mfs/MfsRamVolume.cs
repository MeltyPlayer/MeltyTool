using schema.binary;
using schema.binary.attributes;

namespace marioartist.schema.mfs;

[Flags]
public enum VolumeFlags : byte {
  IS_VOLUME_WRITE_PROTECTED = 1 << 5,
  IS_VOLUME_READ_PROTECTED = 1 << 6,
  IS_WRITE_PROTECTED = 1 << 7,
}

/// <summary>
///   Shamelessly stolen from:
///   https://github.com/LuigiBlood/mfs_manager/blob/master/mfs_library/MFS/MFSDef.cs#L10
/// </summary>
[BinarySchema]
public partial class MfsRamVolume : IBinaryDeserializable {
  public uint Unk0 { get; }
  public uint Unk1 { get; }
  public uint Unk2 { get; }
  public ushort Unk3 { get; }

  public VolumeFlags Flags { get; set; }
  public byte DiskType { get; set; }

  [StringEncoding(StringEncodingType.UTF8)]
  [StringLengthSource(20)]
  public string Name { get; set; }

  public MfsDate Date { get; } = new();

  public ushort Renewal { get; set; }
  public byte Country { get; set; }
  public byte Unk4 { get; set; }
  public uint Unk5 { get; set; }

  public uint Unk6 { get; set; }
  public uint Unk7 { get; set; }
  public uint Unk8 { get; set; }

  [SequenceLengthSource(Mfs.FAT_MAX)]
  public ushort FatEntries { get; set; }

  public IMfsEntry[] MfsEntries { get; private set; }

  [ReadLogic]
  private void ReadEntries_(IBinaryReader br) {
    var mfsEntries = new LinkedList<IMfsEntry>();

    var entryLimit = Mfs.EntryLimit[this.DiskType];
    for (int i = 0; i < entryLimit; ++i) {
      var baseOffset = br.Position = i + 0x16B0 + (i * 0x30);
      var firstByte = br.ReadByte();
      br.Position = baseOffset;

      switch (firstByte & 0xC0) {
        case 0x80: {
          mfsEntries.AddLast(br.ReadNew<MfsDirectory>());
          break;
        }
        case 0x40: {
          mfsEntries.AddLast(br.ReadNew<MfsFile>());
          break;
        }
      }
    }

    this.MfsEntries = mfsEntries.ToArray();
  }
}