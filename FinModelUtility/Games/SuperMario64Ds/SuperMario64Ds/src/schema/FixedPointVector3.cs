using fin.math.xyz;

using schema.binary;
using schema.binary.attributes;

namespace sm64ds.schema;

[BinarySchema]
public partial struct FixedPointVector3 : IBinaryConvertible, IXyz {
  [FixedPoint(1, 19, 12)]
  public float X { get; set; }

  [FixedPoint(1, 19, 12)]
  public float Y { get; set; }

  [FixedPoint(1, 19, 12)]
  public float Z { get; set; }
}