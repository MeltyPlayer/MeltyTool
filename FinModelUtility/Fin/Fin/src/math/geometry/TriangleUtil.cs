using System.Numerics;

namespace fin.math.geometry;

public static class TriangleUtil {
  public static Vector3 CalculateNormal(in Vector3 a,
                                        in Vector3 b,
                                        in Vector3 c)
    => Vector3.Normalize(Vector3.Cross(b - a, c - a));
}