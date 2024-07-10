using System.Numerics;

using fin.animation.interpolation;
using fin.animation.keyframes;

namespace fin.animation.types.vector2;

public class Vector2KeyframeWithTangentsInterpolator
    : Vector2KeyframeWithTangentsInterpolator<KeyframeWithTangents<Vector2>> {
  public static Vector2KeyframeWithTangentsInterpolator Instance { get; }
    = new();

  private Vector2KeyframeWithTangentsInterpolator() { }
}

public class Vector2KeyframeWithTangentsInterpolator<TKeyframe>
    : IKeyframeInterpolator<TKeyframe, Vector2>
    where TKeyframe : IKeyframeWithTangents<Vector2> {
  public Vector2 Interpolate(
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

    return fromCoefficient * from.ValueOut +
           toCoefficient * to.ValueIn +
           Vector2.One * oneCoefficient;
  }
}