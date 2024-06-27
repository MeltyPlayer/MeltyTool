using System.IO;

using fin.io;
using fin.log;
using fin.schema;
using fin.util.asserts;

using schema.binary;
using schema.binary.attributes;

using uni.util.cmd;

namespace uni.platforms.gcn.tools {
  public class RarcDump2 {
    public bool Run(IFileHierarchyFile rarcFile, bool cleanup) {
      Asserts.True(
          rarcFile.Impl.Exists,
          $"Cannot dump RARC because it does not exist: {rarcFile}");

      var finalDirectoryPath = rarcFile.FullNameWithoutExtension;
      if (Directory.Exists(finalDirectoryPath)) {
        return false;
      }

      if (!MagicTextUtil.Verify(rarcFile, "RARC")) {
        return false;
      }

      var directoryPath = rarcFile.FullPath + "_dir";
      if (!Directory.Exists(directoryPath)) {
        var logger = Logging.Create<RarcDump>();
        logger.LogInformation($"Dumping RARC {rarcFile.LocalPath}...");

        // TODO: Is this implementation right? It *seems* to only export the
        // first node in a RARC.
        Files.RunInDirectory(
            rarcFile.Impl.AssertGetParent()!,
            () => {
              ProcessUtil.ExecuteBlockingSilently(
                  GcnToolsConstants.RARCDUMP_EXE,
                  $"\"{rarcFile.FullPath}\"");
            });
        Asserts.True(Directory.Exists(directoryPath),
                     $"Directory was not created: {directoryPath}");
      }

      Directory.Move(directoryPath, finalDirectoryPath);
      if (cleanup) {
        rarcFile.Impl.Delete();
      }

      return true;
    }

    // Based on version 1 of rarcdump by thakis.
    // Expanded with information from: http://wiki.tockdom.com/wiki/RARC_(File_Format)
    private bool Impl_(IFileHierarchyFile rarcFile) {
      var directoryPath = rarcFile.FullPath + "_dir";
      if (Directory.Exists(directoryPath)) {
        //return false;
      }

      var logger = Logging.Create<RarcDump>();
      using var rarcDumpScope = logger.BeginScope("rarcdump");

      logger.LogInformation($"Dumping ARC {rarcFile.LocalPath}...");
      return this.ReadFile_(rarcFile);
    }

    [Unknown]
    private bool ReadFile_(IFileHierarchyFile rarcFile) {
      using var br =
          new SchemaBinaryReader(rarcFile.OpenRead(),
                                 Endianness.BigEndian);

      var header = new RarcHeader();
      header.type = br.ReadString(4);

      if (header.type != "RARC") {
        return false;
      }

      header.size = br.ReadUInt32();
      header.unknown = br.ReadUInt32();
      header.dataStartOffset = br.ReadUInt32();
      br.ReadUInt32s(header.unknown2);

      header.numNodes = br.ReadUInt32();
      header.firstNodeOffset = br.ReadUInt32();
      header.numDirectories = br.ReadUInt32();
      header.fileEntriesOffset = br.ReadUInt32();
      header.stringTableLength = br.ReadUInt32();
      header.stringTableOffset = br.ReadUInt32();
      br.ReadUInt32s(header.unknown5);


      var cwd = Directory.GetCurrentDirectory();

      var directoryPath = rarcFile.FullPath + "_dir";
      {
        var nodes = new RarcNode[header.numNodes];
        for (var i = 0; i < header.numNodes; ++i) {
          nodes[i] = this.GetNode_(br, header, i);
        }

        ;

        foreach (var node in nodes) {
          Directory.SetCurrentDirectory(directoryPath);
          this.DumpNode_(br, node, header);
        }
      }

      Directory.SetCurrentDirectory(cwd);

      return true;
    }

    private RarcNode GetNode_(IBinaryReader br, RarcHeader h, int i) {
      var node = new RarcNode();

      node.type = br.ReadString(StringEncodingType.UTF8, 4);

      var fileNameOffset = br.ReadUInt32();
      var expectedFileNameHash = br.ReadUInt16();
      node.numFileEntries = br.ReadUInt16();
      node.firstFileEntryOffset = br.ReadUInt32();

      var position = br.Position;
      {
        br.Position = 0x20 + h.stringTableOffset + fileNameOffset;
        node.fileName = br.ReadStringNT(StringEncodingType.UTF8);

        var actualFileNameHash = 0;
        foreach (var c in node.fileName) {
          actualFileNameHash *= 3;
          actualFileNameHash += (byte) c;
        }
        Asserts.Equal(expectedFileNameHash,
                      actualFileNameHash,
                      "Node did not have the correct hash!");
      }
      { }
      br.Position = position;

      return node;
    }

    private void DumpNode_(IBinaryReader br, RarcNode node, RarcHeader h) {
      /*
string nodeName = getString(0x20 + n.filenameOffset + h.stringTableOffset, f);
        _mkdir(nodeName.c_str());
        _chdir(nodeName.c_str());

        for (int i = 0; i < n.numFileEntries; ++i) {
          RarcDump.FileEntry curr = getFileEntry(n.firstFileEntryOffset + i, h, f);

          if (curr.id == 0xFFFF) //subdirectory
          {
            if (curr.filenameOffset != 0 &&
                curr.filenameOffset != 2) //don't go to "." and ".."
              dumpNode(getNode(curr.dataOffset, f), h, f);
          } else //file
          {
            string currName =
                getString(curr.filenameOffset + h.stringTableOffset + 0x20, f);
            cout << nodeName << "/" << currName << endl;
            FILE* dest = fopen(currName.c_str(), "wb");

            u32 read = 0;
            u8 buff[1024];
            fseek(f, curr.dataOffset + h.dataStartOffset + 0x20, SEEK_SET);
            while (read < curr.dataSize) {
              int r = fread(buff, 1, min(1024, curr.dataSize - read), f);
              fwrite(buff, 1, r, dest);
              read += r;
            }
            fclose(dest);
          }
        }

        _chdir("..");       */
    }

    [Unknown]
    private FileEntry GetFileEntry_(
        SchemaBinaryReader br,
        RarcHeader header,
        RarcNode node,
        int i) {
      br.Position = 0x20 +
                    header.fileEntriesOffset +
                    node.firstFileEntryOffset +
                    i * 20;

      var fileEntry = new FileEntry();
      fileEntry.id = br.ReadUInt16();
      fileEntry.unknown = br.ReadUInt16();
      fileEntry.unknown2 = br.ReadUInt16();
      fileEntry.filenameOffset = br.ReadUInt16();

      fileEntry.dataOffset = br.ReadUInt32();

      fileEntry.dataSize = br.ReadUInt32();
      fileEntry.zero = br.ReadUInt32();

      return fileEntry;
    }

    public class RarcHeader {
      public string type; //'RARC'
      public uint size; //size of the file
      [Unknown]
      public uint unknown;
      public uint dataStartOffset; //where does the actual data start?
      [Unknown]
      public uint[] unknown2 = new uint[4];

      public uint numNodes;
      public uint firstNodeOffset;
      public uint numDirectories;
      public uint fileEntriesOffset;
      public uint stringTableLength;
      public uint stringTableOffset; //where is the string table stored?
      [Unknown]
      public uint[] unknown5 = new uint[2];
    }

    public class RarcNode {
      public string type;
      public ushort numFileEntries; //how many files belong to this node?
      public uint firstFileEntryOffset;
      public IList<FileEntry> fileEntries;

      public string fileName;
    }

    public class FileEntry {
      public ushort
          id; //file id. If this is 0xFFFF, then this entry is a subdirectory link

      [Unknown]
      public ushort unknown;
      [Unknown]
      public ushort unknown2;
      public ushort filenameOffset; //file/subdir name, offset into string table

      public uint
          dataOffset; //offset to file data (for subdirs: index of Node representing the subdir)

      public uint dataSize; //size of data
      public uint zero; //seems to be always '0'
    };
  }
}