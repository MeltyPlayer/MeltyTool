using System.Runtime.CompilerServices;

using fin.model;

namespace fin.math.interpolation {
  public readonly struct PositionInterpolator : IInterpolator<Position> {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Position Interpolate(Position lhs, Position rhs, float progress)
      => Position.Lerp(lhs, rhs, progress);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Position Interpolate(float fromTime,
                                Position p1,
                                float fromTangent,
                                float toTime,
                                Position p2,
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