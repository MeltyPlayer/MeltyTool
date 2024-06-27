using gx;

using schema.binary;

namespace jsystem.schema.j3dgraph.bmd.mat3;

[BinarySchema]
public partial class BlendFunction : IBlendFunction, IBinaryConvertible {
  public GxBlendMode BlendMode { get; set; }
  public GxBlendFactor SrcFactor { get; set; }
  public GxBlendFactor DstFactor { get; set; }
  public GxLogicOp LogicOp { get; set; }
}