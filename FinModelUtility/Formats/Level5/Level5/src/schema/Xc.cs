using fin.data.dictionaries;
using fin.compression;

using level5.decompression;

using schema.binary;

using LzssDecompressor = level5.decompression.LzssDecompressor;

namespace level5.schema;

public record XcFile(string Name, byte[] Data);

public class Xc : IBinaryDeserializable {
  public ListDictionary<string, XcFile> FilesByExtension { get; } = new();

  public void Read(IBinaryReader br) {
    br.AssertString("XPCK");

    var fileCount = br.ReadUInt16() & 0xfff;

    var fileInfoOffset = br.ReadUInt16() * 4;
    var fileTableOffset = br.ReadUInt16() * 4;
    var dataOffset = br.ReadUInt16() * 4;

    br.ReadUInt16();
    var filenameTableSize = br.ReadUInt16() * 4;

    var hashToData = new Dictionary<uint, byte[]>();
    br.Position = fileInfoOffset;
    for (int i = 0; i < fileCount; i++) {
      var nameCrc = br.ReadUInt32();
      br.ReadInt16();
      var offset = (uint)br.ReadUInt16();
      var size = (uint)br.ReadUInt16();
      var offsetExt = (uint)br.ReadByte();
      var sizeExt = (uint)br.ReadByte();

      offset |= offsetExt << 16;
      size |= sizeExt << 16;
      offset = (uint)(offset * 4 + dataOffset);

      hashToData.Add(nameCrc, br.SubreadAt(offset, () => br.ReadBytes((int) size)));
    }

    var inNameTable = br.SubreadAt(fileTableOffset, () => br.ReadBytes(filenameTableSize));
    if (!new ZlibArrayToArrayDecompressor().TryDecompress(inNameTable, out var nameTable)) {
      nameTable = new LzssDecompressor().Decompress(inNameTable);
    }

    this.FilesByExtension.Clear();
    using (var nt = new SchemaBinaryReader(new MemoryStream(nameTable), br.Endianness)) {
      for (int i = 0; i < fileCount; i++) {
        var name = nt.ReadStringNT();

        var crc = Crc32.Crc32C(name);
        if (hashToData.ContainsKey(crc)) {
          this.FilesByExtension.Add(Path.GetExtension(name), new XcFile(name, hashToData[crc]));
        } else {
          Console.WriteLine("Couldn't find " + name + " " + crc.ToString("X"));
        }
      }
    }
  }
}