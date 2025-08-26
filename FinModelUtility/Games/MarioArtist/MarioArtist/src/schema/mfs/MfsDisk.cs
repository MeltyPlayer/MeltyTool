using fin.util.asserts;

using marioartist.schema.leo;

using schema.binary;

namespace marioartist.schema.mfs;

public enum MfsDiskError {
  NONE,
  INVALID,
  NOT_MFS,
}

public class MfsDisk : IBinaryDeserializable {
  public LeoDisk Disk { get; private set; }
  public MfsDiskError Error { get; private set; }
  public MfsRamVolume? Volume { get; private set; }

  public void Read(IBinaryReader br) {
    this.Disk = new LeoDisk(br);

    this.Volume = null;
    if (this.Disk.Format == LeoDisk.DiskFormat.Invalid) {
      this.Error = MfsDiskError.INVALID;
      return;
    }

    if (this.Disk.RAMFileSystem != LeoDisk.FileSystem.MFS) {
      this.Error = MfsDiskError.NOT_MFS;
      return;
    }

    this.Error = MfsDiskError.NONE;
    using var ramAreaReader = new SchemaBinaryReader(
        this.Disk.GetRAMAreaArray().AssertNonnull(),
        Endianness.BigEndian);
    this.Volume = ramAreaReader.ReadNew<MfsRamVolume>();
  }
}