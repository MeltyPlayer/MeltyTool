using System.Runtime.CompilerServices;

namespace fin.math.interpolation {
  public readonly struct FloatInterpolator : IInterpolator<float> {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public float Interpolate(float fromValue, float toValue, float progress) 
      => float.Lerp(fromValue, toValue, progress);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public float Interpolate(float fromTime,
                             float fromValue,
                             float fromTangent,
                             float toTime,
                             float toValue,
                             float toTangent,
                             float time)
      => HermiteInterpolationUtil.InterpolateFloats(
          fromTime,
          fromValue,
          fromTangent,
          toTime,
          toValue,
          toTangent,
          time);
  }
}