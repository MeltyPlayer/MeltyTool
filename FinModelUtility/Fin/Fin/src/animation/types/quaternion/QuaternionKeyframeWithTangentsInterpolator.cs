using System.Numerics;

using fin.animation.interpolation;
using fin.animation.keyframes;

namespace fin.animation.types.quaternion;

public readonly struct QuaternionKeyframeWithTangentsInterpolator<TKeyframe>
    : IKeyframeInterpolator<TKeyframe, Quaternion>
    where TKeyframe : IKeyframeWithTangents<Quaternion> {
  public Quaternion Interpolate(
      TKeyframe from,
      TKeyframe to,
      float frame,
      ISharedInterpolationConfig sharedInterpolationConfig) {
    InterpolationUtil.GetHermiteCoefficients(
        from,
        to,
        frame,
        sharedInterpolationConfig,
        out var fromCoefficient,
        out var toCoefficient,
        out var oneCoefficient);

    var q1 = from.Value;
    var q2 = to.Value;

    if (Quaternion.Dot(q1, q2) < 0) {
      q2 = -q2;
    }

    // TODO: Is this right??
    return q1 * fromCoefficient +
           q2 * toCoefficient +
           Quaternion.Identity * oneCoefficient;
  }
}