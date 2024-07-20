using schema.binary;

namespace uni.platforms.threeDs.tools.gar.schema;

public class Gar2Subfile : IGarSubfile {
  public string FileName { get; }
  public string FullPath { get; }

  public int Position { get; }
  public int Length { get; }

  public Gar2Subfile(
      IBinaryReader br,
      GarHeader header,
      Gar2FileType fileType,
      int fileInFileTypeIndex) {
    br.Position = fileType.FileListOffset + 4 * fileInFileTypeIndex;
    var fileIndex = br.ReadInt32();

    br.Position = header.FileMetadataOffset + 12 * fileIndex;
    var fileSize = br.ReadInt32();
    var fileNameOffset = br.ReadInt32();
    var fullPathOffset = br.ReadInt32();

    br.Position = fileNameOffset;
    this.FileName = br.ReadStringNT();

    br.Position = fullPathOffset;
    this.FullPath = br.ReadStringNT();

    br.Position = header.DataOffset + 4 * fileIndex;
    var fileOffset = br.ReadInt32();

    this.Position = fileOffset;
    this.Length = Math.Max(fileSize, 0);
  }
}