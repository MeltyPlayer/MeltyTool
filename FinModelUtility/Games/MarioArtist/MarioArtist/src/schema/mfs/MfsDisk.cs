using marioartist.schema.leo;

using schema.binary;

namespace marioartist.schema.mfs;

public class MfsDisk : IBinaryDeserializable {
  public MfsRamVolume? Volume { get; private set; }

  public void Read(IBinaryReader br) {
    var leoDisk = new LeoDisk(br);

    this.Volume = null;
    if (leoDisk.Format == LeoDisk.DiskFormat.Invalid) {
      return;
    }

    if (leoDisk.RAMFileSystem != LeoDisk.FileSystem.MFS) {
      return;
    }

    this.Volume = br.ReadNew<MfsRamVolume>();
  }
}