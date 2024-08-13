using gx;

using schema.binary;
using schema.binary.attributes;


namespace jsystem.schema.j3dgraph.bmd.mat3;

[BinarySchema]
public partial class DepthFunction : IDepthFunction, IBinaryConvertible {
  [IntegerFormat(SchemaIntegerType.BYTE)]
  public bool Enable { get; set; }

  public GxCompareType Func { get; set; }

  [IntegerFormat(SchemaIntegerType.BYTE)]
  public bool WriteNewValueIntoDepthBuffer { get; set; }

  private readonly byte padding_ = byte.MaxValue;
}