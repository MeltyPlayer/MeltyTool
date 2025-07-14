using schema.binary;
using schema.binary.attributes;


namespace marioartist.schema;

[BinarySchema]
[Endianness(Endianness.BigEndian)]
public partial class Tstlt : IBinaryDeserializable {
  public Image Thumbnail { get; } = new Image(24, 24);

  [SequenceLengthSource(12)]
  public uint[] Unk { get; private set; }

  public Image FaceTextures { get; } = new Image(128, 141);
}
