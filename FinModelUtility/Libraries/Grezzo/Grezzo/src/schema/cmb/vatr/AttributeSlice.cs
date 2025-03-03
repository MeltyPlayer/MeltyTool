using schema.binary;
using schema.binary.attributes;

namespace grezzo.schema.cmb.vatr;

[BinarySchema]
public partial class AttributeSlice : IBinaryConvertible {
  public uint Size { get; private set; }
  public uint StartOffset { get; private set; }

  [RAtPosition(nameof(StartOffset))]
  [RSequenceLengthSource(nameof(Size))]
  public byte[] Bytes { get; private set; }
}