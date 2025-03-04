using System.Numerics;

using schema.binary;

namespace pikmin1.schema.mod.collision;

[BinarySchema]
public partial class Plane : IBinaryConvertible {
  public Vector3 position { get; private set; }
  public float diameter;
}