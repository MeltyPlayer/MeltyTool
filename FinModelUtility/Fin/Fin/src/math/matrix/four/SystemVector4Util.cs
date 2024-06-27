using System.Numerics;
using System.Runtime.CompilerServices;

using fin.math.floats;


namespace fin.math.matrix.four;

public static class SystemVector4Util {
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static unsafe bool IsRoughly(this Vector4 lhs, Vector4 rhs) {
    float* lhsPtr = &lhs.X;
    float* rhsPtr = &rhs.X;

    for (var i = 0; i < 4; ++i) {
      if (!lhsPtr[i].IsRoughly(rhsPtr[i])) {
        return false;
      }
    }

    return true;
  }
}