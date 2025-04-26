using fin.animation.interpolation;
using fin.animation.keyframes;

namespace fin.animation.types.radians;

public class RadiansKeyframeWithTangentsInterpolator
    : RadiansKeyframeWithTangentsInterpolator<KeyframeWithTangents<float>> {
  public static RadiansKeyframeWithTangentsInterpolator Instance { get; }
    = new();

  private RadiansKeyframeWithTangentsInterpolator() { }
}

public class RadiansKeyframeWithTangentsInterpolator<TKeyframe>
    : IRadiansKeyframeInterpolator<TKeyframe>
    where TKeyframe : IKeyframeWithTangents<float> {
  public float Interpolate(
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
      return new RadiansKeyframeInterpolator<TKeyframe>().Interpolate(
          from,
          to,
          frame,
          sharedInterpolationConfig);
    }

    var fromValue = from.ValueOut;
    var toValue = to.ValueIn;
    if (!sharedInterpolationConfig.DisableNearestRotationFix) {
      toValue = RadiansKeyframeInterpolator.GetAdjustedToValue(
          fromValue,
          to.ValueIn);
    }

    return fromCoefficient * fromValue +
           toCoefficient * toValue +
           oneCoefficient;
  }

  public bool DisableNearestRotationFix { get; set; }
}