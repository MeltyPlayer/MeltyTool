using fin.schema.color;

using schema.binary;
using schema.binary.attributes;

namespace bar.schema;

/// <summary>
///   "ENvironment"
/// 
///   Shamelessly stolen from:
///   https://github.com/magcius/noclip.website/blob/main/src/BeetleAdventureRacing/ParsedFiles/UVEN.ts
/// </summary>
[BinarySchema]
public sealed partial class Uven : IBinaryDeserializable {
  public Rgb24 ClearColor { get; set; }
  public Rgb24 FogColor { get; set; }

  [SequenceLengthSource(40)]
  public byte[] Unk0 { get; set; }

  [IntegerFormat(SchemaIntegerType.BYTE)]
  public bool ShouldClearScreen { get; set; }

  [SequenceLengthSource(5)]
  public byte[] Unk1 { get; set; }

  private byte UvmdCount { get; set; }

  [SequenceLengthSource(43)]
  public byte[] Unk2 { get; set; }

  [RSequenceLengthSource(nameof(UvmdCount))]
  public UvmdTuple[] Uvmds { get; set; }
}

[BinarySchema]
public sealed partial class UvmdTuple : IBinaryDeserializable {
  public ushort UvmdIndex { get; set; }
  public byte UnkFlags { get; set; }
}