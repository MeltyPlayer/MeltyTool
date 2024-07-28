using fin.io;
using fin.util.asserts;

using schema.binary;
using schema.binary.attributes;

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

      FileStringTableEntry[] entries = Array.Empty<FileStringTableEntry>();
      List<string> strings = [];

      br.Position = header.FileStringTableOffset;
      br.Subread(
          (int) header.FileStringTableSize,
          sbr => {
            var root = sbr.ReadNew<FileStringTableEntry>();
            Asserts.True(root.IsDirectory);

            var numEntries = root.DataSizeOrNextEntryIndex;

            entries = new FileStringTableEntry[numEntries];
            entries[0] = root;
            for (var i = 1; i < numEntries; ++i) {
              entries[i] = sbr.ReadNew<FileStringTableEntry>();
            }

            while (!sbr.Eof) {
              try {
                strings.Add(sbr.ReadStringNT(StringEncodingType.UTF8));
              } catch {}
            }
          });

      this.ProcessEntries_(entries,
                           0,
                           "",
                           br,
                           header.DataTableOffset,
                           strings);
    }

  private void ProcessEntries_(FileStringTableEntry[] entries,
                               int currentEntryIndex,
                               string parentName,
                               IBinaryReader br,
                               uint dataOffset,
                               IList<string> strings) {
      var currentEntry = entries[currentEntryIndex];

      string currentName = parentName;
      if (currentEntryIndex > 0) {
        var namePart = strings[(int) currentEntry.NameOffset];
        currentName = Path.Join(currentName, namePart);
      }

      if (!currentEntry.IsDirectory) {
        // TODO: Write file

        ;

        return;
      }

      var startIndex = currentEntryIndex + 1;
      var endIndex = currentEntry.DataSizeOrNextEntryIndex;
      Asserts.True(entries.Length >= endIndex && startIndex <= endIndex);
      for (var i = startIndex; i < endIndex;) {
        var childEntry = entries[i];
        this.ProcessEntries_(entries,
                             i,
                             currentName,
                             br,
                             dataOffset,
                             strings);

        if (!childEntry.IsDirectory) {
          ++i;
        } else {
          i = (int) childEntry.DataSizeOrNextEntryIndex;
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

[BinarySchema]
public partial class FileStringTableEntry : IBinaryDeserializable {
  [IntegerFormat(SchemaIntegerType.BYTE)]
  public bool IsDirectory { get; set; }

  [IntegerFormat(SchemaIntegerType.UINT24)]
  public uint NameOffset { get; set; }

  public uint DataOffsetOrParentIndex { get; set; }
  public uint DataSizeOrNextEntryIndex { get; set; }
}