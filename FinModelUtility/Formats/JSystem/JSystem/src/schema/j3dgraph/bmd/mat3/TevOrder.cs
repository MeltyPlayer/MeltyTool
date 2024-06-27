using gx;

using schema.binary;

namespace jsystem.schema.j3dgraph.bmd.mat3;

[BinarySchema]
public partial class TevOrder : IBinaryConvertible {
  public byte TexCoordId { get; set; }
  public sbyte TexMap { get; set; }
  public GxColorChannel ColorChannelId { get; set; }
  private readonly byte padding_ = byte.MaxValue;
}