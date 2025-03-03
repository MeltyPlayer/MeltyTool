using gx;

using schema.binary;


namespace jsystem.schema.j3dgraph.bmd.mat3;

[BinarySchema]
public partial class TevOrder : IBinaryConvertible {
  public GxTexCoord TexCoordId { get; set; }
  public GxTexMap TexMap { get; set; }
  public GxColorChannel ColorChannelId { get; set; }
  private readonly byte padding_ = byte.MaxValue;
}