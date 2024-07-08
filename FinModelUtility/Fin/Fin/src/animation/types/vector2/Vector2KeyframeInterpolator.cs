using System.Numerics;

using fin.animation.interpolation;
using fin.animation.keyframes;

namespace fin.animation.types.vector2;

public class Vector2KeyframeInterpolator
    : Vector2KeyframeInterpolator<Keyframe<Vector2>> {
  public static Vector2KeyframeInterpolator Instance { get; } = new();
  private Vector2KeyframeInterpolator() { }
}

public class Vector2KeyframeInterpolator<TKeyframe>
    : IKeyframeInterpolator<TKeyframe, Vector2>
    where TKeyframe : IKeyframe<Vector2> {
  public Vector2 Interpolate(
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