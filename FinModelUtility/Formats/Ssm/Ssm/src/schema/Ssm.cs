using schema.binary;
using schema.binary.attributes;

namespace ssm.schema;

[BinarySchema]
[Endianness(Endianness.BigEndian)]
public partial class Ssm : IBinaryDeserializable {
  public uint HeaderLengthMinus16 { get; set; }
  public uint DataOffset { get; set; }
  public uint DspCount { get; set; }
  public uint StartIndex { get; set; }

  [RSequenceLengthSource(nameof(DspCount))]
  public Dsp[] Dsps { get; set; }
}