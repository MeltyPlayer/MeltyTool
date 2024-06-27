using fin.animation.keyframes;
using fin.math.rotations;

namespace fin.animation.interpolation;

public readonly struct RadianKeyframeWithTangentsInterpolator<TKeyframe>
    : IKeyframeInterpolator<TKeyframe, float>
    where TKeyframe : IKeyframeWithTangents<float> {
  public float Interpolate(
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

    var fromValue = from.Value;
    var toValue = to.Value;

    toValue = fromValue +
              RadiansUtil.CalculateRadiansTowards(fromValue, toValue);

    return fromValue * fromCoefficient +
           toValue * toCoefficient +
           oneCoefficient;
  }
}