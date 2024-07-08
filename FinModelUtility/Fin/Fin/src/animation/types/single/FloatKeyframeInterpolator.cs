using fin.animation.interpolation;
using fin.animation.keyframes;

namespace fin.animation.types.single;

public class FloatKeyframeInterpolator
    : FloatKeyframeInterpolator<Keyframe<float>> {
  public static FloatKeyframeInterpolator Instance { get; } = new();
  private FloatKeyframeInterpolator() { }
}

public class FloatKeyframeInterpolator<TKeyframe>
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

    return from.Value * (1 - t) + to.Value * t;
  }
}