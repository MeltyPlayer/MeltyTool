using schema.binary;
using schema.binary.attributes;

namespace pm.schema;

[BinarySchema]
public sealed partial class Asset : IBinaryConvertible {
  [StringLengthSource(0x10)]
  public string Name { get; set; }

  public uint CompressedDataOffset { get; set; }

  public uint CompressedSize { get; set; }
  public uint UncompressedSize { get; set; }
}
