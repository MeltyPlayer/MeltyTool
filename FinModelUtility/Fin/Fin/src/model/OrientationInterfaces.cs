using System.Numerics;

using fin.math.xy;

using schema.readOnly;

namespace fin.model;

[GenerateReadOnly]
public partial interface IVector2 : IXy {
  float X { get; set; }
  float Y { get; set; }
}

[GenerateReadOnly]
public partial interface IVector4 {
  float X { get; set; }
  float Y { get; set; }
  float Z { get; set; }
  float W { get; set; }
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