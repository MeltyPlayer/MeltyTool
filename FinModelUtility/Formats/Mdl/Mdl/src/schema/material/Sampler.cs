using schema.binary;

namespace mdl.schema.material;

[BinarySchema]
public partial class Sampler : IBinaryConvertible {
  public ushort TextureIndex { get; set; }
  public ushort UnknownIndex { get; set; }
  public byte WrapU { get; set; }
  public byte WrapV { get; set; }
  public byte Unk1 { get; set; }
  public byte Unk2 { get; set; }
}