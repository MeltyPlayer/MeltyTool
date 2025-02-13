using System.Numerics;

using fin.math.xy;

using schema.readOnly;

namespace fin.model;

[GenerateReadOnly]
public partial interface IVector2 : IXy {
  new float X { get; set; }
  new float Y { get; set; }
}

[GenerateReadOnly]
public partial interface IVector4 {
  new float X { get; set; }
  new float Y { get; set; }
  new float Z { get; set; }
  new float W { get; set; }
}

public interface IRotation {
  float XDegrees { get; }
  float YDegrees { get; }
  float ZDegrees { get; }
  IRotation SetDegrees(float x, float y, float z);

  float XRadians { get; }
  float YRadians { get; }
  float ZRadians { get; }
  IRotation SetRadians(float x, float y, float z);

  IRotation SetQuaternion(Quaternion q);
}