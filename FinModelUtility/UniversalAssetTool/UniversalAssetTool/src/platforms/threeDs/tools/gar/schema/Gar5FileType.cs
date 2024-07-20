using schema.binary;

namespace uni.platforms.threeDs.tools.gar.schema;

public class Gar5FileType : IGarFileType {
  public uint FileCount { get; }
  public int FirstFileIndex { get; }
  public int TypeNameOffset { get; }
  public string TypeName { get; }

  public IGarSubfile[] Files { get; }

  public Gar5FileType(
      IBinaryReader br,
      GarHeader header,
      int fileTypeIndex) {
    br.Position = header.FileTypesOffset + 8 * 4 * fileTypeIndex;

    this.FileCount = br.ReadUInt32();
    br.ReadUInt32();
    this.FirstFileIndex = br.ReadInt32();
    this.TypeNameOffset = br.ReadInt32();
    br.ReadInt32();
    br.ReadUInt32();
    br.ReadUInt32();
    br.ReadUInt32();

    br.Position = this.TypeNameOffset;
    this.TypeName = br.ReadStringNT();

    this.Files = new IGarSubfile[this.FileCount];
    for (var i = 0; i < this.FileCount; ++i) {
      this.Files[i] = new Gar5Subfile(br, header, this, i);
    }
  }
}