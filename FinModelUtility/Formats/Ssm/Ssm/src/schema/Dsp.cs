using schema.binary;
using schema.binary.attributes;

namespace ssm.schema;

[BinarySchema]
public sealed partial class Dsp : IBinaryDeserializable, IChildOf<Ssm> {
  public Ssm Parent { get; set; }

  public uint ChannelCount { get; private set; }
  public uint Frequency { get; set; }

  [RSequenceLengthSource(nameof(ChannelCount))]
  public DspChannel[] Channels { get; set; }
}