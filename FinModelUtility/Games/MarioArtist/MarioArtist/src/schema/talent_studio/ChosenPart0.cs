using fin.schema.color;

using schema.binary;


namespace marioartist.schema.talent_studio;

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

  public uint[] UnkForPattern0 { get; } = new uint[2];

  public uint Pattern0SegmentedAddress { get; set; }

  public uint[] Unk2 { get; } = new uint[3];

  public uint Pattern1SegmentedAddress { get; set; }

  public uint[] Unk3 { get; } = new uint[3];

  public uint UnkSegmentedAddress2 { get; set; }

  public uint Unk4 { get; set; }
}