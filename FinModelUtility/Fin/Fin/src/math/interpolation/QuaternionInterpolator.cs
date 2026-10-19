using System.Numerics;
using System.Runtime.CompilerServices;

namespace fin.math.interpolation;

public class SimpleQuaternionInterpolator : IInterpolator<Quaternion> {
  public static SimpleQuaternionInterpolator Instance { get; } = new();

  private SimpleQuaternionInterpolator() { }

  // TODO: Might need to negate q2 if Quaternion.Dot(q1, q2) < 0?
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public Quaternion Interpolate(in Quaternion q1,
                                in Quaternion q2,
                                float progress)
    => Quaternion.Slerp(q1, q2, progress);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public Quaternion Interpolate(float fromTime,
                                in Quaternion fromValue,
                                float fromTangent,
                                float toTime,
                                in Quaternion toValue,
                                float toTangent,
                                float time) {
    // TODO: Figure out how to use tangents here
    var t = (time - fromTime) / (toTime - fromTime);
    return this.Interpolate(fromValue, toValue, t);
  }
}