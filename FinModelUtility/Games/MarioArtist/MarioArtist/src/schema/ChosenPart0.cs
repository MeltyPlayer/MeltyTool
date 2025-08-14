using fin.schema.color;

using schema.binary;
using schema.binary.attributes;


namespace marioartist.schema;

[BinarySchema]
public partial class ChosenColor : IBinaryDeserializable {
  public uint Index { get; set; }
  public Rgba32 Color { get; set; }
}

[BinarySchema]
public partial class ChosenPart0 : IBinaryDeserializable {
  public uint Id { get; set; }
  public uint Unk0 { get; set; }

  public ChosenColor ChosenColor0 { get; } = new();
  public ChosenColor ChosenColor1 { get; } = new();

  [SequenceLengthSource(2)]
  public uint[] Unk1 { get; set; }

  public uint UnkSegmentedAddress0 { get; set; }

  [SequenceLengthSource(3)]
  public uint[] Unk2 { get; set; }

  public uint UnkSegmentedAddress1 { get; set; }

  [SequenceLengthSource(3)]
  public uint[] Unk3 { get; set; }

  public uint UnkSegmentedAddress2 { get; set; }

  public uint Unk4 { get; set; }
}