using fin.util.asserts;

using schema.binary;

namespace uni.platforms.threeDs.tools.gar.schema {
  public class GarHeader {
    public int Version { get; }

    public int Size { get; }

    public short FileTypeCount { get; }
    public short FileCount { get; }

    public int FileTypesOffset { get; }
    public int FileMetadataOffset { get; }
    public int DataOffset { get; }

    public GarHeader(IBinaryReader br) {
      br.AssertString("GAR");

      this.Version = br.ReadByte();
      Asserts.True(this.Version is 2 or 5);

      this.Size = br.ReadInt32();

      this.FileTypeCount = br.ReadInt16();
      this.FileCount = br.ReadInt16();

      this.FileTypesOffset = br.ReadInt32();
      this.FileMetadataOffset = br.ReadInt32();
      this.DataOffset = br.ReadInt32();
    }
  }
}