using fin.archives;

using schema.binary;
using schema.binary.attributes;

namespace nitro.schema.narc;

/// <summary>
///   Shamelessly stolen from:
///   https://github.com/nickworonekin/narchive/blob/master/src/Narchive/Formats/NarcArchive.cs
/// </summary>
[BinarySchema]
[Endianness(Endianness.LittleEndian)]
public partial class Narc : IBinaryDeserializable {
  private readonly string magic0_ = "NARC";
  private readonly uint magic1_ = 0x0100fffe;

  [WSizeOfStreamInBytes]
  private uint fileSize_;

  // TODO: This is an assumption, is this ever anything else?
  private readonly ushort headerLength_ = 0x10;

  // TODO: What is this?
  private readonly ushort unk0_ = 3;

  [Skip]
  public IReadOnlyList<UncompressedArchiveSubFile> FileEntries {
    get;
    private set;
  }

  [ReadLogic]
  private void Read_(IBinaryReader br) {
    var fatbPosition = this.headerLength_;

    // Read the FATB section
    br.Position = fatbPosition;
    br.AssertString("BTAF");

    var fatbLength = br.ReadInt32();
    var fntbPosition = fatbPosition + fatbLength;

    var fileEntryCount = br.ReadInt32();
    var fileEntries = new List<NarcArchiveFileEntry>(fileEntryCount);
    for (var i = 0; i < fileEntryCount; i++) {
      var offset = br.ReadInt32();
      var length = br.ReadInt32() - offset;
      fileEntries.Add(new NarcArchiveFileEntry {
          Offset = offset,
          Length = length,
      });
    }

    // Read the FNTB section
    br.Position = fntbPosition;
    br.AssertString("BTNF");

    var fntbLength = br.ReadInt32();
    var fimgPosition = fntbPosition + fntbLength;

    var hasFilenames = true;

    // If the FNTB length is 16 or less, it's impossible for the entries to have filenames.
    // This section will always be at least 16 bytes long, but technically it's only required to be at least 8 bytes long.
    if (fntbLength <= 16) {
      hasFilenames = false;
    }

    var rootNameEntryOffset = br.ReadInt32();

    // If the root name entry offset is 4, then the entries don't have filenames.
    if (rootNameEntryOffset == 4) {
      hasFilenames = false;
    }

    if (hasFilenames) {
      var rootFirstFileIndex = br.ReadInt16();
      var rootDirectory = new NarcArchiveRootDirectoryEntry();

      var directoryEntryCount
          = br.ReadInt16(); // This includes the root directory
      var directoryEntries
          = new List<NarcArchiveDirectoryEntry>(directoryEntryCount) {
              rootDirectory,
          };

      // This NARC contains filenames and directory names, so read them
      for (var i = 1; i < directoryEntryCount; i++) {
        var nameEntryTableOffset = br.ReadInt32();
        var firstFileIndex = br.ReadInt16();
        var parentDirectoryIndex = br.ReadInt16() & 0xFFF;

        directoryEntries.Add(new NarcArchiveDirectoryEntry {
            Index = i,
            Parent = directoryEntries[parentDirectoryIndex],
            NameEntryOffset = nameEntryTableOffset,
            FirstFileIndex = firstFileIndex,
        });
      }

      NarcArchiveDirectoryEntry currentDirectory = rootDirectory;
      var directoryIndex = 0;
      var fileIndex = 0;
      while (directoryIndex < directoryEntryCount) {
        var entryNameLength = br.ReadByte();
        if ((entryNameLength & 0x80) != 0) {
          // This is a directory name entry
          var entryName = br.ReadString(entryNameLength & 0x7F);
          var entryDirectoryIndex = br.ReadInt16() & 0xFFF;
          var directoryEntry = directoryEntries[entryDirectoryIndex];

          directoryEntry.Name = entryName;
        } else if (entryNameLength != 0) {
          // This is a file name entry
          var entryName = br.ReadString(entryNameLength);
          var fileEntry = fileEntries[fileIndex];

          fileEntry.Directory = directoryEntries[directoryIndex];
          fileEntry.Name = entryName;

          fileIndex++;
        } else {
          // This is the end of a directory
          directoryIndex++;
          if (directoryIndex >= directoryEntryCount) {
            break;
          }

          currentDirectory = directoryEntries[directoryIndex];
        }
      }
    }

    // Read the FIMG section
    br.Position = fimgPosition;
    br.AssertString("GMIF");

    var files = new List<UncompressedArchiveSubFile>();
    this.FileEntries = files;

    if (hasFilenames) {
      foreach (var fileEntry in fileEntries) {
        files.Add(new UncompressedArchiveSubFile(
                      fileEntry.FullName,
                      fimgPosition + 8 + fileEntry.Offset,
                      fileEntry.Length));
      }
    } else {
      throw new NotImplementedException();
    }
  }
}