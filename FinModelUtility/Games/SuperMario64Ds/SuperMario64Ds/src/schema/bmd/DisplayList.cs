using fin.schema;

using schema.binary;
using schema.binary.attributes;

namespace sm64ds.schema.bmd;

[BinarySchema]
public sealed partial class DisplayList : IBinaryConvertible {
  [Unknown]
  public uint Unknown { get; set; }

  public uint DataOffset { get; set; }

  [RAtPosition(nameof(DataOffset))]
  public DisplayListData Data { get; } = new();
}

[BinarySchema]
public sealed partial class DisplayListData : IBinaryConvertible {
  private uint TransformCount { get; set; }
  public uint TransformsOffset { get; set; }

  private uint OpcodesByteLength { get; set; }
  public uint OpcodesOffset { get; set; }

  [RAtPosition(nameof(TransformsOffset))]
  [RSequenceLengthSource(nameof(TransformCount))]
  public byte[] TransformIds { get; set; }

  [RAtPosition(nameof(OpcodesOffset))]
  [RSequenceLengthSource(nameof(OpcodesByteLength))]
  public byte[] OpcodeBytes { get; set; }
}