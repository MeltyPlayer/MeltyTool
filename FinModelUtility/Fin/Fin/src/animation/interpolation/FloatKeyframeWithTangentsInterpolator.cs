using fin.animation.keyframes;

namespace fin.animation.interpolation;

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
    InterpolationUtil.GetHermiteCoefficients(
        from,
        to,
        frame,
        sharedInterpolationConfig,
        out var fromCoefficient,
        out var toCoefficient,
        out var oneCoefficient);

    return fromCoefficient * from.Value +
           toCoefficient * to.Value +
           oneCoefficient;
  }
}