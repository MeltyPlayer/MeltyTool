using schema.binary;
using schema.binary.attributes;

namespace glo.schema;

[BinarySchema]
[Endianness(Endianness.LittleEndian)]
public sealed partial class Glo : IBinaryConvertible {
  private readonly string magic_ = "GLO\0";

  public ushort Version { get; set; }

  [SequenceLengthSource(SchemaIntegerType.UINT16)]
  public GloObject[] Objects { get; set; }
}