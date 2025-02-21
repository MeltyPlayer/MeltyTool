using System.Numerics;

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

  public static explicit operator Vector2(FixedPointVector2 v) => new(v.X, v.Y);
}