using fin.schema.color;

using schema.binary;
using schema.binary.attributes;


namespace marioartist.schema.talent_studio;

[BinarySchema]
[Endianness(Endianness.BigEndian)]
public partial class Tstlt : IBinaryDeserializable {
  public Thumbnail Thumbnail { get; } = new();

  [SequenceLengthSource(12)]
  public uint[] Unk { get; private set; }

  public Argb1555Image FaceTextures { get; } = new(128, 141);

  public AnotherHeader AnotherHeader { get; } = new();
}

[BinarySchema]
public partial class AnotherHeader : IBinaryDeserializable {
  public uint unkCount0;
  public uint unkCount1;
  public uint unk2;
  public Rgba32 SkinColor { get; set; }
}