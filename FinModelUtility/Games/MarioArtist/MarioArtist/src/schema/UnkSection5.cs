using schema.binary;
using schema.binary.attributes;


namespace marioartist.schema;

[BinarySchema]
public partial class UnkSection5 : IBinaryDeserializable {
  public uint UnkAddress { get; set; }

  public uint ChosenPartIndex { get; set; }

  [SequenceLengthSource(0x48)]
  public byte[] Unk0 { get; set; }
}