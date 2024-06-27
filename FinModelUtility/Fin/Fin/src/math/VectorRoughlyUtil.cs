using System.Numerics;

using fin.math.floats;
using fin.math.xyz;
using fin.model;

namespace fin.math;

public static class VectorRoughlyUtil {
  public static bool IsRoughly0(this in Vector2 vector)
    => vector.X.IsRoughly0() &&
       vector.Y.IsRoughly0();

  public static bool IsRoughly0(this IReadOnlyVector2 vector)
    => vector.X.IsRoughly0() &&
       vector.Y.IsRoughly0();

  public static bool IsRoughly1(this in Vector2 vector)
    => vector.X.IsRoughly1() &&
       vector.Y.IsRoughly1();

  public static bool IsRoughly1(this IReadOnlyVector2 vector)
    => vector.X.IsRoughly1() &&
       vector.Y.IsRoughly1();

  public static bool IsRoughly01(this in Vector2 vector)
    => vector.X.IsRoughly0() &&
       vector.Y.IsRoughly1();

  public static bool IsRoughly01(this IReadOnlyVector2 vector)
    => vector.X.IsRoughly0() &&
       vector.Y.IsRoughly1();


  public static bool IsRoughly0(this in Vector3 vector)
    => vector.X.IsRoughly0() &&
       vector.Y.IsRoughly0() &&
       vector.Z.IsRoughly0();

  public static bool IsRoughly1(this in Vector3 vector)
    => vector.X.IsRoughly1() &&
       vector.Y.IsRoughly1() &&
       vector.Z.IsRoughly1();

  public static bool IsRoughly0(this IReadOnlyXyz vec3)
    => vec3.X.IsRoughly0() && vec3.Y.IsRoughly0() && vec3.Z.IsRoughly0();

  public static bool IsRoughly1(this IReadOnlyXyz vec3)
    => vec3.X.IsRoughly1() && vec3.Y.IsRoughly1() && vec3.Z.IsRoughly1();
}