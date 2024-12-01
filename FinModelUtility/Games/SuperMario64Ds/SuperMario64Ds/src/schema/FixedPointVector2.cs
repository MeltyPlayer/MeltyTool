using fin.math.xy;

using schema.binary;
using schema.binary.attributes;

namespace sm64ds.schema;

[BinarySchema]
public partial struct FixedPointVector2 : IBinaryConvertible, IXy {
  [FixedPoint(1, 19, 12)]
  public float X { get; set; }

  [FixedPoint(1, 19, 12)]
  public float Y { get; set; }
}