using System.Numerics;
using System.Runtime.CompilerServices;

namespace fin.math.interpolation {
  public readonly struct Vector3Interpolator : IInterpolator<Vector3> {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Vector3 Interpolate(Vector3 lhs, Vector3 rhs, float progress)
      => Vector3.Lerp(lhs, rhs, progress);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Vector3 Interpolate(float fromTime,
                               Vector3 p1,
                               float fromTangent,
                               float toTime,
                               Vector3 p2,
                               float toTangent,
                               float time) 
      => new(
          HermiteInterpolationUtil.InterpolateFloats(
              fromTime,
              p1.X,
              fromTangent,
              toTime,
              p2.X,
              toTangent,
              time),
          HermiteInterpolationUtil.InterpolateFloats(
              fromTime,
              p1.Y,
              fromTangent,
              toTime,
              p2.Y,
              toTangent,
              time),
          HermiteInterpolationUtil.InterpolateFloats(
              fromTime,
              p1.Z,
              fromTangent,
              toTime,
              p2.Z,
              toTangent,
              time));
  }
}