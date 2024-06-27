using schema.binary;

namespace uni.platforms.threeDs.tools.gar.schema {
  public class Gar5Subfile : IGarSubfile {
    public string FileName { get; }
    public string? FullPath { get; }

    public int Position { get; }
    public int Length { get; }

    public Gar5Subfile(
        IBinaryReader br,
        GarHeader header,
        Gar5FileType fileType,
        int fileInFileTypeIndex) {
      br.Position = header.FileMetadataOffset +
                    (fileType.FirstFileIndex + fileInFileTypeIndex) * 4 * 4;
      var fileSize = br.ReadInt32();
      var fileOffset = br.ReadUInt32();
      var fileNameOffset = br.ReadInt32();
      var fullPathOffset = br.ReadInt32();


      br.Position = fileNameOffset;
      this.FileName = br.ReadStringNT();

      if (fullPathOffset != -1) {
        br.Position = fullPathOffset;
        this.FullPath = br.ReadStringNT();
      }

      if (Path.GetExtension(this.FileName) == string.Empty) {
        this.FileName += $".{fileType.TypeName}";
      }
      if (this.FullPath != null && Path.GetExtension(this.FullPath) == string.Empty) {
        this.FullPath += $".{fileType.TypeName}";
      }

      this.Position = (int) fileOffset;
      this.Length = fileSize;
    }
  }
}