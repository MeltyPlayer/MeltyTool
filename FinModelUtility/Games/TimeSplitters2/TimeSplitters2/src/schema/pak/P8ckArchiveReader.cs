using fin.io;
using fin.io.archive;

using schema.binary;

namespace ts2;

/// <summary>
///   Shamelessly stolen from:
///   https://github.com/OpenRadical/tspak/blob/main/tspak.c
/// </summary>
public partial class P8ckArchiveReader : IArchiveReader<SubArchiveContentFile> {
  private const string MAGIC = "P8CK";

  public bool IsValidArchive(Stream archive)
    => MagicTextUtil.Verify(archive, MAGIC);

  public IArchiveStream<SubArchiveContentFile> Decompress(Stream archive)
    => new SubArchiveStream(archive);

  public IEnumerable<SubArchiveContentFile> GetFiles(
      IArchiveStream<SubArchiveContentFile> archiveStream) {
      var br = archiveStream.AsBinaryReader();

      var header = br.ReadNew<Header>();

      br.Position = header.FileInfoOffset;
      for (var i = 0; i < header.FileInfoCount; ++i) {
        var fileInfo = br.ReadNew<FileInfo>();

        var tmp = br.Position;
        {
          br.Position = header.FileNamesOffset + fileInfo.FileNameOffset;
          var name = br.ReadStringNT();

          yield return new SubArchiveContentFile {
              Position = fileInfo.DataOffset,
              Length = fileInfo.DataLength,
              RelativeName = name
          };
        }
        br.Position = tmp;
      }
    }

  [BinarySchema]
  private partial class Header : IBinaryConvertible {
    private readonly string magic_ = MAGIC;

    public int FileInfoOffset { get; set; }
    public int FileInfoCount { get; set; }
    public int FileNamesOffset { get; set; }
  }

  [BinarySchema]
  private partial class FileInfo : IBinaryConvertible {
    public int FileNameOffset { get; set; }
    public int DataLength { get; set; }
    public int DataOffset { get; set; }
  }
}