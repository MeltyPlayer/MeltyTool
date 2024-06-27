using gx;

using schema.binary;
using schema.binary.attributes;

namespace jsystem.schema.j3dgraph.bmd.mat3;

[BinarySchema]
public partial class AlphaCompare : IAlphaCompare, IBinaryConvertible {
  public GxCompareType Func0 { get; set; }

  [NumberFormat(SchemaNumberType.UN8)]
  public float Reference0 { get; set; }

  public GxAlphaOp MergeFunc { get; set; }
  public GxCompareType Func1 { get; set; }

  [NumberFormat(SchemaNumberType.UN8)]
  public float Reference1 { get; set; }

  public readonly byte padding1_ = byte.MaxValue;
  public readonly byte padding2_ = byte.MaxValue;
  public readonly byte padding3_ = byte.MaxValue;
}