using schema.binary;
using schema.binary.attributes;

namespace glo.schema;

[BinarySchema]
public sealed partial class GloAnimSeg : IBinaryConvertible {
  [StringLengthSource(24)]
  public string Name { get; set; }

  public uint StartFrame { get; set; }
  public uint EndFrame { get; set; }
  public uint Flags { get; set; }
  public float Speed { get; set; }
}