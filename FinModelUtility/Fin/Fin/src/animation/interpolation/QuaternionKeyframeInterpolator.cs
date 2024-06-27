using System.Numerics;

using fin.animation.keyframes;

namespace fin.animation.interpolation;

public readonly struct QuaternionKeyframeInterpolator<TKeyframe>
    : IKeyframeInterpolator<TKeyframe, Quaternion>
    where TKeyframe : IKeyframe<Quaternion> {
  public Quaternion Interpolate(
      TKeyframe from,
      TKeyframe to,
      float frame,
      ISharedInterpolationConfig sharedInterpolationConfig) {
    InterpolationUtil.GetTAndDuration(from,
                                      to,
                                      frame,
                                      sharedInterpolationConfig,
                                      out var t,
                                      out _);

    var q1 = from.Value;
    var q2 = to.Value;

    if (Quaternion.Dot(q1, q2) < 0) {
      q2 = -q2;
    }

    return Quaternion.Slerp(q1, q2, t);
  }
}