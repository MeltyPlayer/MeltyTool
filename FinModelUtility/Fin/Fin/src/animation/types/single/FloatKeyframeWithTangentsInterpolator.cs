using fin.animation.interpolation;
using fin.animation.keyframes;

namespace fin.animation.types.single;

public class FloatKeyframeWithTangentsInterpolator
    : FloatKeyframeWithTangentsInterpolator<KeyframeWithTangents<float>> {
  public static FloatKeyframeWithTangentsInterpolator Instance { get; } = new();
  private FloatKeyframeWithTangentsInterpolator() { }
}

public class FloatKeyframeWithTangentsInterpolator<TKeyframe>
    : IKeyframeInterpolator<TKeyframe, float>
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
      return new FloatKeyframeInterpolator<TKeyframe>().Interpolate(
          from,
          to,
          frame,
          sharedInterpolationConfig);
    }

    return fromCoefficient * from.ValueOut +
           toCoefficient * to.ValueIn +
           oneCoefficient;
  }
}