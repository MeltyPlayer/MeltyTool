using gx;

using schema.binary;


namespace jsystem.schema.j3dgraph.bmd.mat3;

[BinarySchema]
public partial class TevSwapMode : ITevSwapMode, IBinaryConvertible {
  public SwapTableId RasSel { get; set; }
  public SwapTableId TexSel { get; set; }
  private readonly ushort padding_ = ushort.MaxValue;
}