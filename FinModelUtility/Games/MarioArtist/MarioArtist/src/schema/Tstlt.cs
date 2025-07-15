using schema.binary;
using schema.binary.attributes;


namespace marioartist.schema;

[BinarySchema]
[Endianness(Endianness.BigEndian)]
public partial class Tstlt : IBinaryDeserializable {
  public Argb1555Image Thumbnail { get; } = new Argb1555Image(24, 24);

  [SequenceLengthSource(12)]
  public uint[] Unk { get; private set; }

  public Argb1555Image FaceTextures { get; } = new Argb1555Image(128, 141);
}
