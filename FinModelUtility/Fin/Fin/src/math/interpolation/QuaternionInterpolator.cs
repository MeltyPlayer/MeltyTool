using System.Numerics;
using System.Runtime.CompilerServices;

namespace fin.math.interpolation;

public readonly struct QuaternionInterpolator : IInterpolator<Quaternion> {
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public Quaternion Interpolate(
      Quaternion q1,
      Quaternion q2,
      float progress) {
    if (Quaternion.Dot(q1, q2) < 0) {
      q2 = -q2;
    }

    var interp = Quaternion.Slerp(q1, q2, progress);
    return Quaternion.Normalize(interp);
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public Quaternion Interpolate(float fromTime,
                                Quaternion fromValue,
                                float fromTangent,
                                float toTime,
                                Quaternion toValue,
                                float toTangent,
                                float time) {
    // TODO: Figure out how to use tangents here
    var t = (time - fromTime) / (toTime - fromTime);
    return Interpolate(fromValue, toValue, t);
  }
}