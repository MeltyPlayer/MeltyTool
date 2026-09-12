using System;
using System.Numerics;
using System.Runtime.CompilerServices;

using fin.math.floats;


namespace fin.math.matrix.two;

public static class SystemVector2Util {
  public static Vector2 FromRadians(float radians)
    => new(MathF.Cos(radians), MathF.Sin(radians));

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static unsafe bool IsRoughly(this Vector2 lhs, Vector2 rhs) {
    float* lhsPtr = &lhs.X;
    float* rhsPtr = &rhs.X;

    for (var i = 0; i < 2; ++i) {
      if (!lhsPtr[i].IsRoughly(rhsPtr[i])) {
        return false;
      }
    }

    return true;
  }

  public static float ProjectionScalar(this Vector2 a, Vector2 b)
    => Vector2.Dot(a, b) / Vector2.Dot(b, b);


  public static Vector2 Projection(this Vector2 a, Vector2 b)
    => a.ProjectionScalar(b) * b;

  public static Vector2 Blerp(
      in Vector2 topLeft,
      in Vector2 topRight,
      in Vector2 bottomRight,
      in Vector2 bottomLeft,
      double xAmount,
      double yAmount) {
    var topX = topLeft.X * (1 - xAmount) + topRight.X * xAmount;
    var topY = topLeft.Y * (1 - xAmount) + topRight.Y * xAmount;

    var bottomX = bottomLeft.X * (1 - xAmount) + bottomRight.X * xAmount;
    var bottomY = bottomLeft.Y * (1 - xAmount) + bottomRight.Y * xAmount;

    var mergedX = topX * (1 - yAmount) + bottomX * yAmount;
    var mergedY = topY * (1 - yAmount) + bottomY * yAmount;

    return new Vector2((float) mergedX, (float) mergedY);
  }
}