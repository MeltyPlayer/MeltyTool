using schema.binary;

namespace uni.platforms.threeDs.tools.gar.schema;

public class Gar2FileType : IGarFileType {
  public int FileCount { get; }
  public int FileListOffset { get; }
  public int TypeNameOffset { get; }
  public string TypeName { get; }

  public IGarSubfile[] Files { get; }

  public Gar2FileType(
      IBinaryReader br,
      GarHeader header,
      int fileTypeIndex) {
    br.Position = header.FileTypesOffset + 16 * fileTypeIndex;

    this.FileCount = br.ReadInt32();
    this.FileListOffset = br.ReadInt32();
    this.TypeNameOffset = br.ReadInt32();
    br.ReadInt32();

    br.Position = this.TypeNameOffset;
    this.TypeName = br.ReadStringNT();

    this.Files = new IGarSubfile[Math.Max(0, this.FileCount)];
    for (var i = 0; i < this.FileCount; ++i) {
      this.Files[i] = new Gar2Subfile(br, header, this, i);
    }
  }
}