using fin.schema;

using schema.binary;
using schema.binary.attributes;

namespace modl.schema.modl.bw1.node;

[BinarySchema]
public partial class Bw1Material : IBwMaterial, IBinaryConvertible {
  [StringLengthSource(0x10)]
  public string Texture1 { get; set; } = "";

  [StringLengthSource(0x10)]
  public string Texture2 { get; set; } = "";

  [Unknown]
  public byte[] Data { get; } = new byte[0x28];
}