using fin.schema.color;

using schema.binary;
using schema.binary.attributes;


namespace marioartist.schema.talent_studio;

[BinarySchema]
[Endianness(Endianness.BigEndian)]
public partial class Tstlt : IBinaryDeserializable {
  public MfsThumbnail Thumbnail { get; } = new();

  public Header Header { get; } = new();  
}

public enum Gender : ushort {
  GIRL,
  BOY,
  OTHER,
}

[BinarySchema]
public partial class Header : IBinaryDeserializable {
  public uint Checksum { get; set; }
  public uint Unk0 { get; set; }
  [NullTerminatedString]
  private readonly string magic_ = "TSTLT01";

  public uint Unk1 { get; set; }
  [WSizeOfStreamInBytes]
  public uint FileSize { get; set; }
  public uint Unk2 { get; set; }
  public uint HeadSectionLength { get; set; }

  public uint BodySectionLength { get; set; }
  public uint Unk3 { get; set; }
  public uint Unk4 { get; set; }
  // TODO: This is wrong
  public Gender Gender { get; set; }
  public ushort Unk5 { get; set; }
}

[BinarySchema]
public partial class AnotherHeader : IBinaryDeserializable {
  public uint unkCount0;
  public uint unkCount1;
  public uint unk2;
  public Rgba32 SkinColor { get; set; }
}