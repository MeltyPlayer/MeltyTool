using fin.io;
using fin.util.asserts;

using schema.binary;
using schema.binary.attributes;
using schema.util.streams;

namespace uni.games.chibi_robo;

/// <summary>
///   Shamelessly stolen from:
///   - https://github.com/adierking/unplug/blob/main/unplug/src/dvd/archive/reader.rs
///   - https://github.com/adierking/unplug/blob/main/unplug/src/dvd/archive.rs
/// </summary>
public class QpBinArchiveExtractor {
  public void Extract(IReadOnlyGenericFile qpBinFile,
                      ISystemDirectory outDirectory) {
    using var br =
        new SchemaBinaryReader(qpBinFile.OpenRead(), Endianness.BigEndian);
    var header = br.ReadNew<QpBinArchiveHeader>();

    IFileStringTableEntry[] entries = [];
    long stringTableOffset = 0;

    br.Position = header.FileStringTableOffset;
    br.Subread(
        (int) header.FileStringTableSize,
        () => {
          br.AssertByte(1); // isDirectory
          var root = br.ReadNew<FileStringTableDirectory>();

          var numEntries = root.NextEntryIndex;
          entries = new IFileStringTableEntry[numEntries];
          entries[0] = root;
          for (var i = 1; i < numEntries; ++i) {
            var isDirectory = br.ReadByte() != 0;
            entries[i] = isDirectory
                ? br.ReadNew<FileStringTableDirectory>()
                : br.ReadNew<FileStringTableFile>();
          }

          stringTableOffset = br.Position;
        });

    this.ProcessEntries_(outDirectory,
                         entries,
                         0,
                         "",
                         br,
                         stringTableOffset);
  }

  private void ProcessEntries_(
      ISystemDirectory rootDirectory,
      IFileStringTableEntry[] entries,
      int currentEntryIndex,
      string parentName,
      IBinaryReader br,
      long baseStringOffset) {
    var currentEntry = entries[currentEntryIndex];

    string currentName = parentName;
    if (currentEntryIndex > 0) {
      var stringOffset = baseStringOffset + currentEntry.NameOffset;
      br.Position = stringOffset;
      var namePart = br.ReadStringNT(StringEncodingType.UTF8);
      currentName = Path.Join(currentName, namePart);
    }

    switch (currentEntry) {
      case FileStringTableFile fileEntry: {
        var dataOffset = fileEntry.DataOffset;
        var dataSize = fileEntry.DataSize;

        var file = new FinFile(Path.Join(rootDirectory.FullPath, currentName));
        using var fs = file.OpenWrite();
        br.SubreadAt(dataOffset,
                     (int) dataSize,
                     () => br.CopyTo(fs));
        break;
      }
      case FileStringTableDirectory directoryEntry: {
        rootDirectory.GetOrCreateSubdir(currentName);

        var startIndex = currentEntryIndex + 1;
        var endIndex = directoryEntry.NextEntryIndex;
        Asserts.True(entries.Length >= endIndex && startIndex <= endIndex);
        for (var i = startIndex; i < endIndex;) {
          var childEntry = entries[i];
          this.ProcessEntries_(rootDirectory,
                               entries,
                               i,
                               currentName,
                               br,
                               baseStringOffset);

          if (childEntry is FileStringTableDirectory childDirectoryEntry) {
            i = (int) childDirectoryEntry.NextEntryIndex;
          } else if (childEntry is FileStringTableFile) {
            ++i;
          }
        }

        break;
      }
    }
  }
}

[BinarySchema]
public partial class QpBinArchiveHeader : IBinaryDeserializable {
  private readonly uint magic_ = 0x55aa382d;

  public uint FileStringTableOffset { get; set; }
  public uint FileStringTableSize { get; set; }
  public uint DataTableOffset { get; set; }

  private readonly byte[] reserved_ =
      Enumerable.Repeat((byte) 0xcc, 16).ToArray();
}

public interface IFileStringTableEntry {
  public uint NameOffset { get; }
}

[BinarySchema]
public partial class FileStringTableDirectory
    : IFileStringTableEntry, IBinaryDeserializable {
  [IntegerFormat(SchemaIntegerType.UINT24)]
  public uint NameOffset { get; set; }

  public uint ParentIndex { get; set; }
  public uint NextEntryIndex { get; set; }
}

[BinarySchema]
public partial class FileStringTableFile
    : IFileStringTableEntry, IBinaryDeserializable {
  [IntegerFormat(SchemaIntegerType.UINT24)]
  public uint NameOffset { get; set; }

  public uint DataOffset { get; set; }
  public uint DataSize { get; set; }
}