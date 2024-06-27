using fin.animation.keyframes;
using fin.math.rotations;

namespace fin.animation.interpolation;

public readonly struct RadianKeyframeInterpolator<TKeyframe>
    : IKeyframeInterpolator<TKeyframe, float>
    where TKeyframe : IKeyframe<float> {
  public float Interpolate(
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

    var fromValue = from.Value;
    var toValue = to.Value;

    toValue = fromValue +
              RadiansUtil.CalculateRadiansTowards(fromValue, toValue);

    return fromValue * (1 - t) + toValue * t;
  }
}