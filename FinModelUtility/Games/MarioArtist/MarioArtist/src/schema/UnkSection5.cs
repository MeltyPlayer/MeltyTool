using schema.binary;
using schema.binary.attributes;


namespace marioartist.schema;

[BinarySchema]
public partial class UnkSection5 : IBinaryDeserializable {
  [SequenceLengthSource(4)]
  public SubUnkSection5[] Subs { get; set; }
}

[BinarySchema]
public partial class SubUnkSection5 : IBinaryDeserializable {
  public uint UnkAddress { get; set; }
  public uint ChosenPartId { get; set; }

  [SequenceLengthSource(3)]
  public uint[] Unk0 { get; set; }
}