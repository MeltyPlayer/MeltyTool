using System.Runtime.CompilerServices;

using fin.math.rotations;

namespace fin.math.interpolation {
  public readonly struct RadianInterpolator : IInterpolator<float> {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public float Interpolate(float fromValue, float toValue, float progress) {
      toValue = fromValue +
                RadiansUtil.CalculateRadiansTowards(fromValue, toValue);
      return float.Lerp(fromValue, toValue, progress);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public float Interpolate(float fromTime,
                             float fromValue,
                             float fromTangent,
                             float toTime,
                             float toValue,
                             float toTangent,
                             float time) {
      toValue = fromValue +
                RadiansUtil.CalculateRadiansTowards(fromValue, toValue);

      return HermiteInterpolationUtil.InterpolateFloats(
          fromTime,
          fromValue,
          fromTangent,
          toTime,
          toValue,
          toTangent,
          time);
    }
  }
}