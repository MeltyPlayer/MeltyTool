using System.Numerics;

using fin.animation.interpolation;
using fin.animation.keyframes;

namespace fin.animation.types.quaternion;

public class QuaternionKeyframeWithTangentsInterpolator
    : QuaternionKeyframeInterpolator<Keyframe<Quaternion>> {
  public static QuaternionKeyframeWithTangentsInterpolator Instance { get; }
    = new();

  private QuaternionKeyframeWithTangentsInterpolator() { }
}

public class QuaternionKeyframeWithTangentsInterpolator<TKeyframe>
    : IKeyframeInterpolator<TKeyframe, Quaternion>
    where TKeyframe : IKeyframeWithTangents<Quaternion> {
  public Quaternion Interpolate(
      TKeyframe from,
      TKeyframe to,
      float frame,
      ISharedInterpolationConfig sharedInterpolationConfig) {
    if (!InterpolationUtil.TryToGetHermiteCoefficients(
            from,
            to,
            frame,
            sharedInterpolationConfig,
            out var fromCoefficient,
            out var toCoefficient,
            out var oneCoefficient)) {
      return new QuaternionKeyframeInterpolator<TKeyframe>().Interpolate(
          from,
          to,
          frame,
          sharedInterpolationConfig);
    }

    var q1 = from.ValueOut;
    var q2 = to.ValueIn;

    if (Quaternion.Dot(q1, q2) < 0) {
      q2 = -q2;
    }

    // TODO: Is this right??
    return q1 * fromCoefficient +
           q2 * toCoefficient +
           Quaternion.Identity * oneCoefficient;
  }
}