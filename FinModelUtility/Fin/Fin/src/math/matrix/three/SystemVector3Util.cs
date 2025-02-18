using System.Numerics;
using System.Runtime.CompilerServices;

using fin.math.floats;
using fin.math.rotations;
using fin.util.hash;


namespace fin.math.matrix.three;

public static class SystemVector3Util {
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static unsafe bool IsRoughly(this Vector3 lhs, Vector3 rhs) {
    float* lhsPtr = &lhs.X;
    float* rhsPtr = &rhs.X;

    for (var i = 0; i < 3; ++i) {
      if (!lhsPtr[i].IsRoughly(rhsPtr[i])) {
        return false;
      }
    }

    return true;
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static int GetRoughHashCode(this Vector3 v)
    => FluentHash.Start()
                 .With(v.X.GetRoughHashCode())
                 .With(v.Y.GetRoughHashCode())
                 .With(v.Z.GetRoughHashCode());

  public static Vector3 FromPitchYawRadians(float pitchRadians,
                                            float yawRadians) {
    FinTrig.FromPitchYawRadians(pitchRadians,
                                yawRadians,
                                out var x,
                                out var y,
                                out var z);
    return new Vector3(x, y, z);
  }
}