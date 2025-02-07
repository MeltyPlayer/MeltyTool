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
}