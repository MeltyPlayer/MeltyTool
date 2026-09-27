using fin.schema;

using schema.binary;
using schema.binary.attributes;

namespace berserkDc.schema.bml;

[BinarySchema]
public sealed partial class BmlEntry : IBinaryDeserializable {
  [StringLengthSource(20)]
  public string Name { get; set; }

  public uint CompressedSize { get; set; }

  [Unknown]
  public uint Unknown { get; set; }

  public uint DecompressedSize { get; set; }
  public uint PvmCompressed { get; set; }
  public uint PvmDeompressed { get; set; }

  private uint padding0_;
  private uint padding1_;
  private uint padding2_;
}