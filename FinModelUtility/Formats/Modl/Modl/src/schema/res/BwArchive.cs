using fin.data.dictionaries;

using modl.schema.res.texr;

using schema.binary;
using schema.binary.attributes;

namespace modl.schema.res;

public class BwArchive : IBinaryDeserializable {
  public TexrSection TexrSection { get; } = new();
  public Sond Sond { get; } = new();

  public ListDictionary<string, BwFile> Files { get; } = new();

  public void Read(IBinaryReader br) {
    this.TexrSection.Read(br);
    this.Sond.Read(br);

    this.Files.Clear();

    while (!br.Eof) {
      var bwFile = br.ReadNew<BwFile>();
      this.Files.Add(bwFile.Type, bwFile);
    }
  }
}

[BinarySchema]
public partial class Sond : IBinaryConvertible {
  private readonly string magic_ = "DNOS"; // SOND backwards

  [SequenceLengthSource(SchemaIntegerType.UINT32)]
  public byte[] Data { get; private set; }
}

public class BwFile : IBinaryDeserializable {
  public string Type { get; private set; }
  public string FileName { get; private set; }
  public byte[] Data { get; private set; }

  public void Read(IBinaryReader br) {
    SectionHeaderUtil.ReadNameAndSize(
        br,
        out var sectionName,
        out var dataLength);
    this.Type = sectionName;
    var dataOffset = br.Position;

    this.FileName = br.ReadString(br.ReadInt32());

    br.Position = dataOffset;
    this.Data = br.ReadBytes((int) dataLength);

    br.Position = dataOffset + dataLength;
  }

  public void Write(IBinaryWriter bw) =>
      throw new NotImplementedException();
}