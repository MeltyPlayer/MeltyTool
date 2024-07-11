using System.Numerics;

using fin.animation.interpolation;
using fin.animation.keyframes;

namespace fin.animation.types.quaternion;

public class QuaternionKeyframeInterpolator
    : QuaternionKeyframeInterpolator<Keyframe<Quaternion>> {
  public static QuaternionKeyframeInterpolator Instance { get; } = new();
  private QuaternionKeyframeInterpolator() { }
}

public class QuaternionKeyframeInterpolator<TKeyframe>
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

    var q1 = from.ValueOut;
    var q2 = to.ValueIn;

    if (Quaternion.Dot(q1, q2) < 0) {
      q2 = -q2;
    }

    return Quaternion.Slerp(q1, q2, t);
  }
}