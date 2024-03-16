using schema.binary;
using schema.binary.attributes;

namespace visceral.schema.mtlb {
  [BinarySchema]
  public partial class Mtlb : IBinaryConvertible {
    [SequenceLengthSource(0x44)]
    public byte[] unknown;

    public uint SomeOffset { get; set; }

    public uint ChannelsOffset { get; set; }
  }
}
