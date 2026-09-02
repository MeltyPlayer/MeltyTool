using fin.archives;
using fin.data.stacks;
using fin.io;
using fin.schema;

using schema.binary;
using schema.binary.attributes;

namespace uni.platforms.gcn.tools;

public sealed record GcmArchiveFileBundle(IReadOnlyTreeFile MainFile)
    : ISimpleArchiveFileBundle;

/// <summary>
///   Shamelessly ported from version 1.0 (20050213) of gcmdump by thakis.
/// </summary>
public partial class GcmArchiveImporter
    : BSimpleArchiveImporter<GcmArchiveFileBundle> {
  protected override void BuildHierarchyAndGetFileStream(
      GcmArchiveFileBundle bundle,
      ISet<IReadOnlyGenericFile> fileSet,
      ISimpleArchiveDirectory builderRoot,
      out Stream baseStream,
      out Stream readStream) {
    var rawRomStream = bundle.MainFile.OpenRead();

    var isCiso = MagicTextUtil.Verify(rawRomStream, "CISO");
    rawRomStream.Position = 0;

    baseStream = readStream =
        !isCiso ? rawRomStream : new CisoStream(rawRomStream);

    var br = new SchemaBinaryReader(readStream, Endianness.BigEndian);

    var diskHeader = br.ReadNew<DiskHeader>();
    var fileEntries = this.ReadFileSystemTable_(br, diskHeader);

    var directories = new ISimpleArchiveDirectory[fileEntries.Count];
    directories[0] = builderRoot;

    //for now, dump directory structure
    var directoryStack =
        new FinStack<(ISimpleArchiveDirectory, uint lastChildIndex)>(
            (builderRoot, (uint) fileEntries.Count));

    var fileTableOffset = 12 * fileEntries.Count;
    for (int i = 1; i < fileEntries.Count; ++i) {
      var e = fileEntries[i];

      // Pop to reach parent directory
      while (i >= directoryStack.Top.lastChildIndex) {
        directoryStack.Pop();
      }

      // Get name
      br.Position = diskHeader.FileSystemTableOffset +
                    fileTableOffset +
                    e.FileNameOffset;
      var name = br.ReadStringNT(StringEncodingType.UTF8);

      // Push new directory
      if (e.IsDirectory) {
        var parentDir = directories[e.FileOrParentOffset];
        var childDir = parentDir.AddSubdir(name);

        directories[i] = childDir;
        directoryStack.Push((childDir, e.FileLengthOrNextOffset));
      }
      // Export file
      else {
        var position = (int) e.FileOrParentOffset;
        var length = (int) e.FileLengthOrNextOffset;

        directoryStack.Top.Item1.AddFile(name, position, length);
      }
    }
  }

  private IList<FileEntry> ReadFileSystemTable_(IBinaryReader br,
                                                DiskHeader diskHeader) {
    var entries = new List<FileEntry>();

    //read files
    br.Position = diskHeader.FileSystemTableOffset;
    uint numFiles = 1;
    for (int i = 0; i < numFiles; ++i) {
      var entry = br.ReadNew<FileEntry>();
      entries.Add(entry);
      if (i == 0) {
        numFiles = entry.FileLengthOrNextOffset;
      }
    }

    return entries;
  }

  [BinarySchema]
  private partial class Ids : IBinaryConvertible {
    public byte ConsoleId { get; set; }
    public ushort GameId { get; set; }
    public byte CountryId { get; set; }
    public ushort MakerId { get; set; }
  }

  [BinarySchema]
  private partial class DiskHeader : IBinaryConvertible {
    public Ids Ids { get; } = new();
    public byte DiskId { get; set; }
    public byte Version { get; set; }
    public byte AudioStreaming { get; set; }
    public byte StreamBufferSize { get; set; }

    [Unknown]
    [SequenceLengthSource(0x12)]
    public byte[] Unused { get; set; }

    [StringLengthSource(4)]
    public string DvdMagicWord { get; set; }

    [StringLengthSource(0x3e0)]
    public string GameName { get; set; }

    public uint DebugMonitorOffset { get; set; }
    public uint DebugLoadAddress { get; set; }

    [Unknown]
    [SequenceLengthSource(0x18)]
    public byte[] Unused2 { get; set; }

    public uint DolOffset { get; set; }

    public uint FileSystemTableOffset { get; set; }
    public uint FileSystemTableSize { get; set; }
    public uint FileSystemTableMaximumSize { get; set; }

    public uint UserPosition { get; set; }
    public uint UserLength { get; set; }

    [Unknown]
    public uint Unknown { get; set; }

    [Unknown]
    public uint Unused3 { get; set; }
  }

  [BinarySchema]
  private partial class FileEntry : IBinaryConvertible {
    [IntegerFormat(SchemaIntegerType.BYTE)]
    public bool IsDirectory { get; set; }

    [IntegerFormat(SchemaIntegerType.UINT24)]
    public uint FileNameOffset { get; set; }

    public uint FileOrParentOffset { get; set; }
    public uint FileLengthOrNextOffset { get; set; }
  }
}