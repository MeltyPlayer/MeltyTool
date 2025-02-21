using fin.schema;

using schema.binary;
using schema.binary.attributes;

namespace sm64ds.schema.bmd;

[BinarySchema]
public partial class DisplayList : IBinaryConvertible {
  [Unknown]
  public uint Unknown { get; set; }

  public uint DataOffset { get; set; }

  [RAtPosition(nameof(DataOffset))]
  public DisplayListData Data { get; } = new();
}

[BinarySchema]
public partial class DisplayListData : IBinaryConvertible {
  public uint TransformCount { get; set; }
  public uint TransformsOffset { get; set; }

  public uint OpcodesByteLength { get; set; }
  public uint OpcodesOffset { get; set; }

  [RAtPosition(nameof(TransformsOffset))]
  [RSequenceLengthSource(nameof(TransformCount))]
  public byte[] TransformIds { get; set; }

  [RAtPosition(nameof(OpcodesOffset))]
  [RSequenceLengthSource(nameof(OpcodesByteLength))]
  public byte[] OpcodeBytes { get; set; }
}